using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Constellation.Drone.Downloader.Converter;
using Constellation.Drone.Downloader.DownloadClient;
using System.Text;
using System.Text.Json;

namespace Constellation.Drone.Downloader
{
    public class WatcherPayload
    {
        public string? PayloadType { get; set; }
        public string? Url { get; set; }
        public string? QualitySelection { get; set; }
    }

    public class DroneClient
    {
        private Dictionary<string, IDownloader> _operations = new Dictionary<string, IDownloader>()
        {
            { "youtubedl", new YTDLDownloader() }
        };

        QueueClient? queue { get; set; }

        public DroneClient() { }

        public DroneClient(string connectionString, string queueName) {
            queue = new QueueClient(connectionString, queueName);
            queue.CreateIfNotExists();
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

        public void EnqueueWork(WatcherPayload payload)
        {
            if (queue != null)
            {
                var message = JsonSerializer.Serialize<WatcherPayload>(payload);

                if (message != null)
                {
                    var bodyString = Base64Encode(message);

                    queue.SendMessage(bodyString);
                }
            }
        }

        public async Task DoWork(WatcherPayload payload)
        {
            LoggingClient.Log($"Invoking {payload.PayloadType} against URL {payload.Url} with quality of {payload.QualitySelection}.");
            if (payload.PayloadType != null)
            {
                var handler = _operations[payload.PayloadType];

                var result = await handler.Download(payload);

                if (result != null)
                {
                    var conversion = MediaConverter.GetMediaConverter(result);

                    if (conversion != null)
                    {
                        var conversionResult = conversion.Convert(result);
                    }
                }

                // todo: add in call to LiteClient and log the workResult!
            }
            else
            {
                _e("Null handlertype is unacceptable!");
            }
        }

        private void _e(string message)
        {
            LoggingClient.Log(message);
            Environment.Exit(1);
        }

        public Tuple<QueueMessage?, WatcherPayload?>? GetWork()
        {
            if (queue != null)
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

                    if (result != null)
                    {
                        payload = result;
                    }
                    else
                    {
                        _e($"Unable to deserialize \"{jsonString}\"");
                    }

                }
                catch (Exception ex)
                {
                    LoggingClient.Log("Unable to get queueMessage");
                    _e(ex.Message);
                }

                try
                {
                    if (queueMessage != null)
                    {
                        queue.DeleteMessage(queueMessage.MessageId, queueMessage.PopReceipt);
                    }
                }
                catch (Exception)
                {
                    LoggingClient.Log("Unable to cleanup queue message, continuing.");
                }

                return new Tuple<QueueMessage?, WatcherPayload?>(queueMessage, payload);
            }

            return null;
        }
    }
}
