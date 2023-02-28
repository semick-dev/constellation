using System;
using System.Collections.Generic;
using System.Linq;
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

            processHandler.Run("youtube-dl", $"-f {payload.QualitySelection} -v --write-info-json {payload.Url}", Configuration.DownloadDirectory);
        }
    }
}
