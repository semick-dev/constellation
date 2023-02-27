using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        public void DoWork(WatcherPayload payload)
        {

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

        public WatcherPayload GetWork()
        {
            QueueMessage? queueMessage = null;
            WatcherPayload payload = new WatcherPayload();

            try
            {
                queueMessage = queue.ReceiveMessage().Value;
                var jsonString = Base64Decode(Encoding.UTF8.GetString(queueMessage.Body));

                var document = JsonDocument.Parse(jsonString);

                var result = JsonSerializer.Deserialize<WatcherPayload>(document);

                if (result != null) {
                    payload = result;
                }
                else
                {
                    Console.WriteLine($"Unable to deserialize \"{jsonString}\"");
                    Environment.Exit(1);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to get queueMessage");
                Console.WriteLine(ex.Message);
                Environment.Exit(1);
            }

            try
            {
                queue.DeleteMessage(queueMessage.MessageId, queueMessage.PopReceipt);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unable to cleanup queue message, continuing.");
                Console.WriteLine(ex.Message);
            }

            return payload;
        }
    }
}
