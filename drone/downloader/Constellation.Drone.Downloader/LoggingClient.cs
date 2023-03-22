using Azure.Storage.Blobs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Constellation.Drone.Downloader
{
    public static class LoggingClient
    {
        private static System.Timers.Timer WriteBufferCaller = new System.Timers.Timer();
        private static List<string> Buffer = new List<string>();
        public static BlobContainerClient? BlobClient { get; private set; }


        public static DateTime GetWindowTime(DateTime? input)
        {
            DateTime inputDate = input ?? System.DateTime.UtcNow;
            var inputDateRounded = new DateTime(inputDate.Year, inputDate.Month, inputDate.Day, inputDate.Hour, inputDate.Minute, 0);

            var iterations = 0;
            while (true)
            {
                if (iterations != 0)
                {
                    if (inputDateRounded.Minute == 0)
                    {
                        return inputDateRounded;
                    }

                    if (inputDateRounded.Minute % 5 == 0)
                    {
                        return inputDateRounded;
                    }

                    inputDateRounded = inputDateRounded.AddMinutes(1);
                }

                iterations++;
            }
        }

        public static string FormatLogName(DateTime inputDateTime)
        {
            return inputDateTime.ToString("yyyyMMddHHmm");
        }

        public static void Flush()
        {
            WriteBuffer();
        }

        public static void WriteBuffer(object? source = null, ElapsedEventArgs? e = null)
        {
            StringBuilder buffer = new StringBuilder();
            List<string> bufferItems = new List<string>();
            
            lock(Buffer)
            {
                bufferItems = Buffer.ToList();
                Buffer.Clear();
            }

            foreach(var data in bufferItems)
            {
                buffer.AppendLine(data);
            }

            var fileName = $"{FormatLogName(GetWindowTime(null))}.txt";
            var remoteFileUri = $"logs/{fileName}";
            var localFileUri = Path.Combine(Configuration.LogDirectory, fileName);

            try
            {
                if (!Directory.Exists(Configuration.LogDirectory))
                {
                    Directory.CreateDirectory(Configuration.LogDirectory);
                }

                using (FileStream fs = File.Open(localFileUri, FileMode.Append))
                {
                    fs.Write(Encoding.UTF8.GetBytes(buffer.ToString()));
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            if (BlobClient != null)
            {
                try
                {
                    var blobClient = BlobClient.GetBlobClient(remoteFileUri);

                    blobClient.Upload(localFileUri, overwrite: true);
                }
                catch(Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }
        }

        public static void SetConnectionString(string connectionSAS, string containerName)
        {
            try
            {
                BlobClient = new BlobContainerClient(connectionSAS, containerName);

                // Hook up the Elapsed event for the timer.
                WriteBufferCaller.Elapsed += new ElapsedEventHandler(WriteBuffer);

                // Set the Interval to 2 seconds (2000 milliseconds).
                WriteBufferCaller.Interval = 10000;
                WriteBufferCaller.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to create queues/blobs client. Possible bad connection string value \"{connectionSAS}\"");
                Console.WriteLine(ex.Message);
            }
        }

        public static void Log(string? message)
        {
            Console.WriteLine(message);

            if (message != null)
            {
                Buffer.Add(message);
            }
        }
    }
}
