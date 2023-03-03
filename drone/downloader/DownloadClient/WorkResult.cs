using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Drone.Downloader.DownloadClient
{
    public class WorkResult
    {
        public WatcherPayload Payload { get; set; }

        public string ThumbnailUri { get; set; }

        public string DownloadUri { get; set; }

        
        public WorkResult(WatcherPayload payload, string thumbnailUri, string downloadUri)
        {
            Payload = payload;
            ThumbnailUri = thumbnailUri;
            DownloadUri = downloadUri;
        }
    }
}
