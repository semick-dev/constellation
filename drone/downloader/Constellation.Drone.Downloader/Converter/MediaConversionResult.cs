using Constellation.Drone.Downloader.DownloadClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Constellation.Drone.Downloader.ProcessHandler;

namespace Constellation.Drone.Downloader.Converter
{
    public class MediaConversionResult
    {
        /// <summary>
        /// The completed workload of a download operation. The payload to Convert that results in this class!
        /// </summary>
        WorkResult InputWorkload { get; set; }

        /// <summary>
        /// After conversion, the file will exist on disk in the download directory. This is the name of that new file.
        /// </summary>
        public string OutputFile { get; set; }

        /// <summary>
        /// If calling an external process for the conversion, this is the result of that process!
        /// </summary>
        public ProcessResult ConversionResult { get; set; }

        public MediaConversionResult(WorkResult completedWork, ProcessResult conversionProcess, string convertedFilename)
        {
            InputWorkload = completedWork;
            ConversionResult = conversionProcess;
            OutputFile = convertedFilename;
        }
    }
}
