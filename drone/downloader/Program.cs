using Azure.Storage.Queues;
using Constellation.Drone.Downloader.DownloadClient;
using System;

namespace Constellation.Drone.Downloader
{
    internal class Program
    {
        static int Main(string[] args)
        {
            
            var connectionString = args[0];
            string queueName = "watcher";
            DroneClient client = new DroneClient();

            try
            {
                client = new DroneClient(connectionString, queueName);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Unable to create queues/blobs client. Possible bad connection string value \"{args[0]}\"");
                Console.Write(ex.Message);
            }
            
            // exceptions handled here
            var work = client.GetWork();

            try
            {
                if (work != null && work.Item2 != null)
                {
                    client.DoWork(work.Item2);
                }
                else
                {
                    Console.WriteLine("No work to be executed.");
                }
            }
            catch(RetriableMessageException ex)
            {
                Console.WriteLine(ex.Message);
                if (work != null && work.Item2 != null)
                {
                    client.EnqueueWork(work.Item2);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Ran into an unexpected and uncaught error. Work not re-enqueued");
                Console.WriteLine(ex.Message);
                Console.Write(ex.StackTrace);
                return 1;
            }

            return 0;
        }
    }
}