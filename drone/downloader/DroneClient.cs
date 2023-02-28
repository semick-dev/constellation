﻿using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Constellation.Drone.Downloader.DownloadClient;
using System.Text;
using System.Text.Json;

namespace Constellation.Drone.Downloader
{
    public class WatcherPayload
    {
        public string? PayloadType { get; set; }
        public string? QualitySelection { get; set; }
        public string? Url { get; set; }
    }

    public class DroneClient
    {
        private Dictionary<string, IDownloader> _operations = new Dictionary<string, IDownloader>()
        {
            { "youtubedl", new YTDLDownloader() }
        };

        QueueClient queue { get; set; }

        public DroneClient(string connectionString, string queueName) {
            try
            {
                queue = new QueueClient(connectionString, queueName);
                queue.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create initial queue connection.");
                Console.WriteLine(ex.ToString());
                Environment.Exit(1);
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public void EnqueueWork()
        {
            var payload = new WatcherPayload()
            {
                PayloadType = "youtubedl",
                QualitySelection = "bestaudio",
                Url = "https://youtu.be/Cnas3DmIMt0?list=FL7Nyaz8DK7bhCUA3ZcvvrmA"
            };

            var message = JsonSerializer.Serialize<WatcherPayload>(payload);

            if (message != null) {
                var bodyString = Base64Encode(message);

                queue.SendMessage(bodyString);
            }
        }

        public void DoWork(WatcherPayload payload)
        {
            Console.WriteLine($"Invoking {payload.PayloadType} against URL {payload.Url} with quality of {payload.QualitySelection}.");

            try
            {
                if (payload.PayloadType != null)
                {
                    var handler = _operations[payload.PayloadType];

                    handler.Download(payload);
                }
                else
                {
                    _e("Null handlertype is unacceptable!");
                }
            }
            catch(Exception e)
            {
                _e(e.ToString());
            }
        }

        private void _e(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(1);
        }

        public WatcherPayload? GetWork()
        {
            QueueMessage? queueMessage = null;
            WatcherPayload payload = new WatcherPayload();

            try
            {
                 queueMessage = queue.ReceiveMessage().Value;

                if (queueMessage == null)
                {
                    return null;
                }

                var jsonString = Base64Decode(Encoding.UTF8.GetString(queueMessage.Body));

                var document = JsonDocument.Parse(jsonString);

                var result = JsonSerializer.Deserialize<WatcherPayload>(document);

                if (result != null) {
                    payload = result;
                }
                else
                {
                    _e($"Unable to deserialize \"{jsonString}\"");
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to get queueMessage");
                _e(ex.Message);
            }

            try
            {
                if(queueMessage != null)
                {
                    queue.DeleteMessage(queueMessage.MessageId, queueMessage.PopReceipt);
                }
            }
            catch(Exception)
            {
                Console.WriteLine("Unable to cleanup queue message, continuing.");
            }

            return payload;
        }
    }
}
