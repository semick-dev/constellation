using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Drone.Downloader.DownloadClient
{
    public class WorkResult
    {
        /// <summary>
        /// The original payload pulled deserialized from the work queue.
        /// </summary>
        public WatcherPayload Payload { get; set; }

        /// <summary>
        /// The youtube video thumbnail (in highest quality) saved with the format {videoId}.jpg.
        /// </summary>
        public string ThumbnailUri { get; set; }

        /// <summary>
        /// After the download is completed, the file will be written with the format {videoId}.{downloadExtension}.
        /// 
        /// 
        /// </summary>
        public string DownloadUri { get; set; }

        public WorkResult(WatcherPayload payload, string thumbnailUri, string downloadUri)
        {
            Payload = payload;
            ThumbnailUri = thumbnailUri;
            DownloadUri = downloadUri;
        }
    }
}
