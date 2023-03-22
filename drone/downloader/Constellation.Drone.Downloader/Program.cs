using Azure.Storage.Queues;
using Constellation.Drone.Downloader.DownloadClient;
using System;

namespace Constellation.Drone.Downloader
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            
            var connectionString = args[0];
            string containerQueueName = "watcher";
            DroneClient client = new DroneClient();

            try
            {
                client = new DroneClient(connectionString, containerQueueName);
            }
            catch(Exception ex)
            {
                LoggingClient.Log($"Unable to create queues/blobs client. Possible bad connection string value \"{args[0]}\"");
                LoggingClient.Log(ex.Message);
                LoggingClient.Flush();
                return 1;
            }
            LoggingClient.SetConnectionString(connectionString, containerQueueName);

            // exceptions handled here
            var work = client.GetWork();

            try
            {
                if (work != null && work.Item2 != null)
                {
                    await client.DoWork(work.Item2);
                }
                else
                {
                    LoggingClient.Log("No work to be executed.");
                }
            }
            catch(RetriableMessageException ex)
            {
                LoggingClient.Log(ex.Message);
                if (work != null && work.Item2 != null)
                {
                    client.EnqueueWork(work.Item2);
                }
            }
            catch(Exception ex)
            {
                LoggingClient.Log(ex.Message);
                LoggingClient.Log(ex.StackTrace);

                LoggingClient.Flush();
                return 1;
            }

            LoggingClient.Flush();
            return 0;
        }
    }
}