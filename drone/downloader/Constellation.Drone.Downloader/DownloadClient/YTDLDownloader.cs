﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Constellation.Drone.Downloader.DownloadClient
{
    public class YTDLDownloader : IDownloader
    {
        public YTDLDownloader() { }
        private ProcessHandler processHandler = new ProcessHandler();

        public async Task<WorkResult?> Download(WatcherPayload payload)
        {
            if (!Directory.Exists(Configuration.DownloadDirectory))
            {
                Directory.CreateDirectory(Configuration.DownloadDirectory);
            }

            if (payload.Url != null)
            {
                string thumbnailUri = await DownloadThumbnail(payload);
                string downloadFormat = "%(id)s.%(ext)s";
                string downloadUri = string.Empty;

                if (IsAlreadyDownloaded(payload.Url)) {

                    LoggingClient.Log($"Discovered an info.json for \"${ExtractVideoId(payload.Url)}\" with downloaded bit set to true. Skipping download.");
                    return null;
                }

                var result = processHandler.Run("youtube-dl", $"-f {payload.QualitySelection} -v --write-info-json -o \"{downloadFormat}\" {payload.Url}", Configuration.DownloadDirectory);
                downloadUri = GetDownloadUri(payload.Url);

                return new WorkResult(payload, thumbnailUri, downloadUri);
            }
            else
            {
                throw new Exception("Unable to do work on a payload with no URL.");
            }
        }

        public bool IsAlreadyDownloaded(string url)
        {
            string videoId = ExtractVideoId(url);
            var infoJson = Path.Combine(Configuration.DownloadDirectory, $"{videoId}.info.json");
            
            if (File.Exists(infoJson))
            {
                try
                {
                    var node = GetInfoJson(url);

                    var downloadedBit = node.Root["downloaded"];
                    if (downloadedBit != null)
                    {
                        if (downloadedBit.ToString().ToLower() == "true")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception e)
                {
                    LoggingClient.Log(e.Message);
                    LoggingClient.Log("Can't find downloaded bit in info json. Hit an exception accessing, redownloading!");
                    return false;
                }
            }

            return false;
        }

        private JsonNode GetInfoJson(string url)
        {
            string videoId = ExtractVideoId(url);
            var infoJson = Path.Combine(Configuration.DownloadDirectory, $"{videoId}.info.json");
            string ext = string.Empty;

            using (StreamReader r = new StreamReader(infoJson))
            {
                string json = r.ReadToEnd();
                var jsonNode = JsonObject.Parse(json);

                if (jsonNode != null)
                {
                    return jsonNode;
                }
                else
                {
                    throw new Exception("Unable to open an info.json for this file.");
                }
            }
        }

        /// <summary>
        /// Uses the .info.json provided by a successful youtubedl invocation to provide a location of the resulting file.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string GetDownloadUri(string url)
        {
            string videoId = ExtractVideoId(url);
            var infoJson = Path.Combine(Configuration.DownloadDirectory, $"{videoId}.info.json");
            string ext = string.Empty;

            var jsonNode = GetInfoJson(url);

            if (jsonNode != null)
            {
                // todo: ready to add title to name if necessary
                var possibleTitle = jsonNode.Root["title"];
                var possibleExt = jsonNode.Root["ext"];
                if (possibleTitle != null)
                {
                    ext = possibleTitle.ToString();
                }
                if (possibleExt != null)
                {
                    ext = possibleExt.ToString();
                }
            }

            return Path.Combine(Configuration.DownloadDirectory, $"{videoId}.{ext}");
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

        private async Task<byte[]> getImageBytes(string videoId)
        {
            byte[] result = new byte[0];
            string[] urlsInDescendingOrder = new string[]
            {
                $"https://img.youtube.com/vi/{videoId}/maxresdefault.jpg",
                $"https://img.youtube.com/vi/{videoId}/sddefault.jpg",
                $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg",
                $"https://img.youtube.com/vi/{videoId}/mqdefault.jpg",
                $"https://img.youtube.com/vi/{videoId}/default.jpg",
            };

            using (var client = new HttpClient())
            {
                foreach (var url in urlsInDescendingOrder)
                {
                    try
                    {
                        return await client.GetByteArrayAsync(new Uri(url));
                    }
                    catch (Exception) { }
                }
            }

            return result;
        }

        private async Task<string> DownloadThumbnail(WatcherPayload payload)
        {
            if (payload.Url != null)
            {
                var videoId = ExtractVideoId(payload.Url);
                var targetFile = Path.Combine(Configuration.DownloadDirectory, $"{videoId}.jpg");

                byte[] imageBytes = new byte[0];

                imageBytes = await getImageBytes(videoId);
                    
                if (imageBytes.Length == 0)
                {
                    LoggingClient.Log($"Unable to download any quality thumbnail for video id \"videoId\"");
                }
                else
                {
                    try
                    {
                        await File.WriteAllBytesAsync(targetFile, imageBytes);
                        return targetFile;
                    }
                    catch (Exception e)
                    {
                        LoggingClient.Log($"Unable to write image bytes to {targetFile}");
                        LoggingClient.Log(e.Message);
                    }
                }
            }
            else
            {
                LoggingClient.Log("Unable to download thumbnail for nonexistent video id.");
            }

            return string.Empty;
        }
    }
}
