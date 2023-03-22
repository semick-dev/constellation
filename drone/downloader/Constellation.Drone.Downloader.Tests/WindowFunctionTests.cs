using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Constellation.Drone.Downloader.Tests
{
    public class WindowFunctionTests
    {
        [Theory]
        [InlineData("2022-10-10 03:21:59", "2022-10-10 03:25")]
        [InlineData("2022-10-10 23:59:24", "2022-10-11 00:00")]
        [InlineData("2022-10-11 23:59:23", "2022-10-12 00:00")]
        [InlineData("2022-03-22 10:00:00 AM", "2022-10-12 00:00")]
        public void TestWindowFunction(string inputTime, string expectedWindow)
        {
            DateTime input = DateTime.Parse(inputTime);
            DateTime expected = DateTime.Parse(expectedWindow);

            var windowTime = LoggingClient.GetWindowTime(input);
            Assert.Equal(expected, windowTime);
        }

        [Theory]
        [InlineData("2022-10-10 23:59:24", "202210102359")]
        [InlineData("2022-10-11 23:59:23", "202210112359")]
        public void TestFormatString(string inputTime, string expected)
        {
            DateTime input = DateTime.Parse(inputTime);
            var result = LoggingClient.FormatLogName(input);

            Assert.Equal(expected, result);
        }
    }
}