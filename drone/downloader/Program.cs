using Azure.Storage.Queues;
using System;

namespace Constellation.Drone.Downloader
{
    internal class Program
    {
        static int Main(string[] args)
        {
            var connectionString = args[0];
            string queueName = "watcher";

            var client = new DroneClient(connectionString, queueName);
            var work = client.GetWork();

            if (work != null)
            {
                client.DoWork(work);
            }
            else
            {
                Console.WriteLine("No work to be executed.");
            }

            return 0;
        }
    }
}