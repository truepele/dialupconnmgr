using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DotRas;

namespace dialupconnmgr
{
    class StatisticsLogger
    {
        private string _path;
        private System.Xml.XmlWriter _writer;
        private List<StatisticsLogEntry> _entriesList = new List<StatisticsLogEntry>();
        SemaphoreSlim _semaphor = new SemaphoreSlim(1,1);

        private XmlSerializer _serializer;

        public StatisticsLogger(string path = "Statistics.xml")
        {
            _path = path;
            using (XmlReader reader = new XmlTextReader(_path))
            {
                var origin = _serializer.Deserialize(reader) as StatisticsLogger;
                if (origin != null)
                {
                    _entriesList = new List<StatisticsLogEntry>(origin.EntriesList);
                }
            }

            _writer = new XmlTextWriter(_path, Encoding.UTF8);
            _serializer = new XmlSerializer(typeof(StatisticsLogger));
        }

        [XmlElement]
        public List<StatisticsLogEntry> EntriesList
        {
            get { return _entriesList; }
            private set { _entriesList = value; }
        }

        public async Task Log(DateTime fixationTime, int rashandleHash, string username, string phonebookentryName, RasLinkStatistics statistics, double upspeed, double downspeed)
        {
            await _semaphor.WaitAsync();

            StatisticsLogEntry entry;
            if (!EntriesList.Any() || EntriesList.Last().RashandleHash != rashandleHash)
            {
                entry = new StatisticsLogEntry(rashandleHash, username, phonebookentryName);
                EntriesList.Add(entry);

            }
            else
            {
                entry = EntriesList.Last();
            }

            entry.Update(fixationTime, statistics, upspeed, downspeed);

            _semaphor.Release();
        }

        public async Task Save()
        {
            await _semaphor.WaitAsync();
            _semaphor.Release();
            _serializer.Serialize(_writer, this);
        }
       
        public void Dispose()
        {
           _writer.Dispose();
        }
    }
}
