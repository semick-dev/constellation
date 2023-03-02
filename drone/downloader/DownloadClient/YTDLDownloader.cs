using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Drone.Downloader.DownloadClient
{
    public class YTDLDownloader : IDownloader
    {
        public YTDLDownloader() { }
        private ProcessHandler processHandler = new ProcessHandler();

        public void Download(WatcherPayload payload)
        {
            if (!Directory.Exists(Configuration.DownloadDirectory))
            {
                Directory.CreateDirectory(Configuration.DownloadDirectory);
            }

            DownloadThumbnail(payload);
            processHandler.Run("youtube-dl", $"-f {payload.QualitySelection} -v --write-info-json {payload.Url}", Configuration.DownloadDirectory);
        }

        public static string ExtractVideoId(string url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                var uri = new Uri(url);

                // https://youtu.be/tNw8y3EQAtI?list=FL7Nyaz8DK7bhCUA3ZcvvrmA;
                // https://youtu.be/y8BUc_aYZ-M;
                if (url.StartsWith("https://youtu.be"))
                {
                    return uri.AbsolutePath.Substring(1);
                }

                // https://www.youtube.com/watch?v=y8BUc_aYZ-M
                // https://www.youtube.com/watch?v=9PLPfL6Tge4&list=PLYFEK0EdxB0oSLDSttKm6aAI-lxI4G3FD
                else if (url.StartsWith("https://www.youtube.com"))
                {
                    var queries = uri.Query.Split("&");
                    foreach (var query in queries)
                    {
                        if (query.Contains("v=")){
                            return query.Split("=")[1];
                        }
                    }
                }

                else return "";
            }

            return "";
        }

        private async void DownloadThumbnail(WatcherPayload payload)
        {
            if (payload.Url != null)
            {
                var videoId = ExtractVideoId(payload.Url);
                var thumbnailUrl = $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg";
                var targetFile = Path.Combine(Configuration.DownloadDirectory, $"{videoId}.jpg");

                using (var client = new HttpClient()) {
                    byte[] imageBytes = new byte[0];
                    try
                    {
                        imageBytes = await client.GetByteArrayAsync(new Uri(thumbnailUrl));
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine($"Unable to download thumbnail for video id \"{videoId}\".");
                        Console.WriteLine(e.Message);
                    }
                    
                    try
                    {
                        await File.WriteAllBytesAsync(targetFile, imageBytes);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Unable to write image bytes to {targetFile}");
                        Console.WriteLine(e.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("Unable to download thumbnail for nonexistent video id.");
            }
            
        }
    }
}
