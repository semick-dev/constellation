namespace Constellation.Drone.Downloader
{
    public static class Configuration
    {
        public static string WorkingDirectory { get
            {
                var environmentSetting = Environment.GetEnvironmentVariable("DRONE_DOWNLOADER_DIRECTORY");
                return environmentSetting ?? Environment.CurrentDirectory;
            }
        }

        public static string LogDirectory { get
            {
                var environmentSetting = Environment.GetEnvironmentVariable("DRONE_DOWNLOADER_LOG_DIRECTORY");
                return environmentSetting ?? Path.Join(Environment.CurrentDirectory, "log");
            } 
        }

        public static string DownloadDirectory { get
            {
                var environmentSetting = Environment.GetEnvironmentVariable("DRONE_DOWNLOADER_DOWNLOAD_DIRECTORY");
                return environmentSetting ?? Path.Join(Environment.CurrentDirectory, "download");
            }
        }
    }
}
