using Constellation.Drone.Downloader.DownloadClient;

namespace Constellation.Drone.Downloader.Converter
{
    public static class MediaConverter
    {
        public static IMediaConverter? GetMediaConverter(WorkResult completedWork)
        {
            switch (Path.GetExtension(completedWork.DownloadUri))
            {
                case ".webm":
                    return new WebMConverter();
                default: 
                    return null;
            }
        }
    }
}
