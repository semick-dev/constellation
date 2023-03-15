using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constellation.Drone.Downloader
{
    public class RetriableMessageException : Exception
    {
        public RetriableMessageException(string message) : base(message) { }
    }
}
