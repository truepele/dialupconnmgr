using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.TextFormatting;
using System.Xml.Serialization;
using DotRas;

namespace dialupconnmgr
{

    public class StatisticsLogEntry
    {
        private ulong _speedupCount = 0;
        private ulong _speeddownCount = 0;
        private double _sumUpspeed;
        private double _sumDownspeed;

        public StatisticsLogEntry()
        {
            ConnectionDuration = new TimeSpan();
        }

        public StatisticsLogEntry(int rashandleHash, string username, string phonebookentryName)
        {
            RashandleHash = rashandleHash;
            UserName = username;
            PhoneBookEntryName = phonebookentryName;
        }

        public StatisticsLogEntry(int rashandleHash, string username, string phonebookentryName, DateTime fixationTime, RasLinkStatistics statistics):this(rashandleHash, username, phonebookentryName)
        {
            Update(fixationTime, statistics);
        }

        public StatisticsLogEntry(int rashandleHash, string username, string phonebookentryName, DateTime fixationTime, RasLinkStatistics statistics, double upspeed, double downspeed)
            : this(rashandleHash, username, phonebookentryName, fixationTime, statistics)
        {
            UpdateSpeed(upspeed, downspeed);
        }

        public void UpdateSpeed(double upspeed, double downspeed)
        {
            LastUpSpeed = upspeed;
            LastDownSpeed = downspeed;
            

            if (upspeed > 0)
            {
                if (MaxUpSpeed < upspeed)
                    MaxUpSpeed = upspeed;

                _sumUpspeed += upspeed;
                _speedupCount++;
                AvgUpSpeed = _sumUpspeed/_speedupCount;
            }

            if (downspeed > 0)
            {
                if (MaxDownSpeed < downspeed)
                    MaxDownSpeed = downspeed;

                _sumDownspeed += downspeed;
                _speeddownCount++;
                AvgDownSpeed = _sumDownspeed / _speeddownCount;
            }
        }

        public void Update(DateTime fixationTime, RasLinkStatistics statistics, double upspeed, double downspeed)
        {
            Update(fixationTime, statistics);
            UpdateSpeed(upspeed, downspeed);
        }

        private void Update(DateTime fixationTime, RasLinkStatistics statistics)
        {
            ConnectionDuration = statistics.ConnectionDuration;
            BytesReceived = statistics.BytesReceived;
            BytesTransmitted = statistics.BytesTransmitted;

            LastFixationTime = fixationTime;
            if (ConnectedTime == DateTime.MinValue)
                ConnectedTime = LastFixationTime - ConnectionDuration;
            if (FirstFixationTime == DateTime.MinValue)
                FirstFixationTime = fixationTime;
        }

        [XmlAttribute]
        public int RashandleHash { get; set; }
        [XmlAttribute]
        public DateTime LastFixationTime { get; set; }
        [XmlAttribute]
        public DateTime FirstFixationTime { get; set; }
        [XmlAttribute]
        private DateTime ConnectedTime { get; set; }

        [XmlIgnore]
        public TimeSpan ConnectionDuration { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public long ConnectionDurationSeconds
        {
            get { return (long)ConnectionDuration.TotalSeconds; }
            set
            {
                ConnectionDuration = new TimeSpan(TimeSpan.TicksPerSecond * value);
            }
        }



        //[XmlAttribute(Type=typeof(string))]public 

        [XmlAttribute]
        public long BytesReceived { get; set; }
        [XmlAttribute]
        public long BytesTransmitted { get; set; }
        [XmlAttribute]
        public double LastUpSpeed { get; set; }
        [XmlAttribute]
        public double LastDownSpeed { get; set; }
        [XmlAttribute]
        public double AvgUpSpeed { get; set; }
        [XmlAttribute]
        public double AvgDownSpeed { get; set; }
        [XmlAttribute]
        public double MaxUpSpeed { get; set; }
        [XmlAttribute]
        public double MaxDownSpeed { get; set; }
        [XmlAttribute]
        public string UserName { get; set; }
        [XmlAttribute]
        public string PhoneBookEntryName { get; set; }
    }
}
