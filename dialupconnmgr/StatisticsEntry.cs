using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dialupconnmgr
{
    public struct StatisticsEntry
    {
        public TimeSpan Duration;
        public double BytesReceived;
        public double BytesTransmitted;
    }
}
