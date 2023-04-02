using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Drone.Downloader.DownloadClient
{
    public interface IDownloader
    {
        public Task<WorkResult?> Download(WatcherPayload payload);
    }
}
