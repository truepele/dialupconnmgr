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
        private double _sumUpspeed;
        private double _sumDownspeed;

        public StatisticsLogEntry()
        {
            AggregatedCount = 1;
            SpeedupPointcount = 0;
            SpeeddownPointcount = 0;
            ConnectionDuration = new TimeSpan();
        }

        public StatisticsLogEntry(int rashandleHash, string username, string phonebookentryName)
        {
            SpeedupPointcount = 0;
            SpeeddownPointcount = 0;
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

                _sumUpspeed = _sumUpspeed + upspeed;
                SpeedupPointcount++;
                AvgUpSpeed = _sumUpspeed / SpeedupPointcount;
            }

            if (downspeed > 0)
            {
                if (MaxDownSpeed < downspeed)
                    MaxDownSpeed = downspeed;

                _sumDownspeed += downspeed;
                SpeeddownPointcount++;
                AvgDownSpeed = _sumDownspeed / SpeeddownPointcount;
            }
        }

        public void Update(DateTime fixationTime, RasLinkStatistics statistics, double upspeed, double downspeed)
        {
            Update(fixationTime, statistics);
            UpdateSpeed(upspeed, downspeed);
        }

        public void SetDisconnected(DateTime fixationTime)
        {
            LastFixationTime = fixationTime;
            DisconnectTime = ConnectedTime + ConnectionDuration;
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

        public static StatisticsLogEntry operator +(StatisticsLogEntry e1, StatisticsLogEntry e2)
        {
            var less = e1.ConnectedTime < e2.ConnectedTime ? e1 : e2;
            var larger = e1.ConnectedTime < e2.ConnectedTime ? e2 : e1;

            var result = new StatisticsLogEntry()
            {
                AvgDownSpeed = (e1.SpeeddownPointcount * e1.AvgDownSpeed + e2.SpeeddownPointcount * e2.AvgDownSpeed) / (e1.SpeeddownPointcount + e2.SpeeddownPointcount),
                AvgUpSpeed =
                (e1.SpeedupPointcount * e1.AvgUpSpeed + e2.SpeedupPointcount * e2.AvgUpSpeed) / (e1.SpeedupPointcount + e2.SpeedupPointcount),
                BytesReceived = e1.BytesReceived+e2.BytesReceived,
                BytesTransmitted = e1.BytesTransmitted + e2.BytesTransmitted,
                ConnectedTime = less.ConnectedTime,
                ConnectionDuration = e1.ConnectionDuration + e2.ConnectionDuration,
                FirstFixationTime = less.FirstFixationTime,
                LastFixationTime = larger.LastFixationTime,
                LastDownSpeed = larger.LastDownSpeed,
                LastUpSpeed = larger.LastUpSpeed,
                SpeeddownPointcount = e1.SpeeddownPointcount+e2.SpeeddownPointcount,
                SpeedupPointcount = e1.SpeedupPointcount + e2.SpeedupPointcount,
                MaxDownSpeed = Math.Max(e1.MaxDownSpeed, e2.MaxDownSpeed),
                MaxUpSpeed = Math.Max(e1.MaxUpSpeed, e2.MaxUpSpeed),
                AggregatedCount = e1.AggregatedCount+e2.AggregatedCount
            };

            return result;
        }

        [XmlAttribute("Id")]
        public int RashandleHash { get; set; }

        [XmlAttribute("lastfix")]
        public DateTime LastFixationTime { get; set; }

        [XmlAttribute("firstfix")]
        public DateTime FirstFixationTime { get; set; }

        [XmlAttribute("connectime")]
        public DateTime ConnectedTime { get; set; }

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
        
        [XmlAttribute("recieved")]
        public long BytesReceived { get; set; }

        [XmlAttribute("transmitted")]
        public long BytesTransmitted { get; set; }

        [XmlAttribute("lastUSpeed")]
        public double LastUpSpeed { get; set; }

        [XmlAttribute("lastDSpeed")]
        public double LastDownSpeed { get; set; }

        [XmlAttribute("avgUSpeed")]
        public double AvgUpSpeed { get; set; }

        [XmlAttribute("avgDSpeed")]
        public double AvgDownSpeed { get; set; }

        [XmlAttribute("MaxUSpeed")]
        public double MaxUpSpeed { get; set; }

        [XmlAttribute("MaxDSpeed")]
        public double MaxDownSpeed { get; set; }

        [XmlAttribute("UserName")]
        public string UserName { get; set; }

        [XmlAttribute("EntryName")]
        public string PhoneBookEntryName { get; set; }

        [XmlAttribute("USpeedCount")]
        public ulong SpeedupPointcount { get; set; }

        [XmlAttribute("DSpeedCount")]
        public ulong SpeeddownPointcount { get; set; }

        [XmlAttribute("DisconnectTime")]
        public DateTime DisconnectTime { get; set; }

        [XmlIgnore]
        public int AggregatedCount { get; set; }

        [XmlIgnore]
        public List<StatisticsLogEntry> Childs { get; set; }
    }
}
