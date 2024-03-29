﻿using Constellation.Drone.Downloader.DownloadClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Drone.Downloader.Converter
{
    public class WebMConverter : IMediaConverter
    {
        public ProcessHandler ProcessHandler { get; set; }

        public WebMConverter()
        {
            ProcessHandler = new ProcessHandler();
        }

        public bool ToolPresent { get
            {
                ProcessHandler handler = new ProcessHandler();
                
                var result = handler.Run("ffmpeg", "-h", Configuration.WorkingDirectory);

                return result.ExitCode == 0;
            } 
        }

        public string GetOutputUrl(WorkResult completedWork)
        {
            var directory = Path.GetDirectoryName(completedWork.DownloadUri);

            return Path.Combine(directory ?? string.Empty, $"{Path.GetFileNameWithoutExtension(completedWork.DownloadUri)}.m4a");
        } 

        public MediaConversionResult Convert(WorkResult completedWork)
        {
            var inputUri = completedWork.DownloadUri;
            var outputUri = GetOutputUrl(completedWork);

            if (ToolPresent)
            {
                var result = new ProcessResult()
                {
                    ExitCode = 0,
                    StdOut = "Output already exists! Skipping."
                };

                if (!File.Exists(outputUri))
                {
                    result = ProcessHandler.Run("ffmpeg", $"-i \"{inputUri}\" -vn \"{outputUri}\"", Configuration.DownloadDirectory);
                }
                else
                {
                    LoggingClient.Log(result.StdOut);
                }

                if (result.ExitCode == 0)
                {
                    return new MediaConversionResult(completedWork, result, outputUri);
                }
                else
                {
                    LoggingClient.Log("Conversion exited without exit code 0. Throwing an unretriable exception. ");
                }
            }
            else
            {
                LoggingClient.Log($"Unable to run conversion of {inputUri} to {outputUri} because ffmpeg is not present on this machine.");
            }

            return new MediaConversionResult(completedWork, new ProcessResult(), string.Empty);
        }
    }
}
