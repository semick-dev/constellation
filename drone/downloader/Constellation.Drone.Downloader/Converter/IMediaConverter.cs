using Constellation.Drone.Downloader.DownloadClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Drone.Downloader.Converter
{
    public interface IMediaConverter
    {
        public MediaConversionResult Convert(WorkResult completedWork);
    }
}
