using Azure.Storage.Queues;
using System;

namespace Constellation.Drone.Downloader
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var connectionString = args[0];
            string queueName = "watcher";

            var client = new DroneClient(connectionString, queueName);
            client.EnqueueWork();
            var work = client.GetWork();
            client.DoWork(work);
        }
    }
}