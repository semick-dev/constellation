using Constellation.Drone.Downloader.DownloadClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Constellation.Drone.Downloader.Tests
{
    public class DroneTests
    {

        [Fact]
        public void Test_IsAlreadyDownloaded_Positive()
        {
            var downloader = new YTDLDownloader();
            var testDownloadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestEntries");

            Environment.SetEnvironmentVariable("DRONE_DOWNLOADER_DOWNLOAD_DIRECTORY", testDownloadDirectory);

            var result = downloader.IsAlreadyDownloaded("https://youtu.be/downloaded");

            Assert.True(result);
        }

        [Fact]
        public void Test_IsAlreadyDownloaded_Negative()
        {
            var downloader = new YTDLDownloader();
            var testDownloadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestEntries");

            Environment.SetEnvironmentVariable("DRONE_DOWNLOADER_DOWNLOAD_DIRECTORY", testDownloadDirectory);

            var result = downloader.IsAlreadyDownloaded("https://youtu.be/undownloaded");

            Assert.False(result);
        }

        [Fact]
        public void Test_IsAlreadyDownloaded_NoFile()
        {
            var downloader = new YTDLDownloader();
            var testDownloadDirectory = Path.Combine(Directory.GetCurrentDirectory(), "TestEntries");

            Environment.SetEnvironmentVariable("DRONE_DOWNLOADER_DOWNLOAD_DIRECTORY", testDownloadDirectory);

            var result = downloader.IsAlreadyDownloaded("https://youtu.be/nonexistentinfojson");

            Assert.False(result);
        }
    }
}
