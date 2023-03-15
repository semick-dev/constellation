using Constellation.Drone.Downloader.DownloadClient;
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
                
                var result = handler.Run("ffmpeg", "", Configuration.WorkingDirectory);

                return result.ExitCode == 0;
            } 
        }

        public string GetOutputUrl(WorkResult completedWork)
        {
            return $"{Path.GetFileNameWithoutExtension(completedWork.DownloadUri)}.m4a";
        } 

        public MediaConversionResult Convert(WorkResult completedWork)
        {
            var inputUri = completedWork.DownloadUri;
            var outputUri = GetOutputUrl(completedWork);

            if (ToolPresent)
            {
                var result = ProcessHandler.Run("ffmpeg", $"-i '{inputUri}' -vn '{outputUri}'", Configuration.DownloadDirectory);

                if (result.ExitCode == 0)
                {
                    return new MediaConversionResult(completedWork, result, outputUri);
                }
                else
                {
                    Console.WriteLine("Conversion exited without exit code 0. Throwing an unretriable exception. ");
                }
            }
            else
            {
                Console.WriteLine($"Unable to run conversion of {inputUri} to {outputUri} because ffmpeg is not present on this machine.");
            }

            return new MediaConversionResult(completedWork, new ProcessResult(), string.Empty);
        }
    }
}
