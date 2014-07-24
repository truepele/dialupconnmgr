using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace dialupconnmgr
{
    public class StatisticsLogBase
    {
        protected List<StatisticsLogEntry> _entriesList = new List<StatisticsLogEntry>();
        protected SemaphoreSlim _semaphor = new SemaphoreSlim(1,1);
        protected string _directoryPath;
        protected XmlSerializer _serializer;

        public StatisticsLogBase()
        {
        }

        public StatisticsLogBase(string directoryPath = null)
        {
            _serializer = new XmlSerializer(typeof(StatisticsLogger));
            _directoryPath = !string.IsNullOrEmpty(directoryPath) ? Path.GetDirectoryName(directoryPath)
               : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
        }

        protected List<StatisticsLogEntry> ReadEntries(DateTime date)
        {
            var path = ConstructPath(date, _directoryPath);
            List<StatisticsLogEntry> entries = null;

            if (File.Exists(path))
            {
                using (XmlReader reader = new XmlTextReader(path))
                {
                    try
                    {
                        var origin = _serializer.Deserialize(reader) as StatisticsLogger;
                        if (origin != null)
                        {
                            entries = new List<StatisticsLogEntry>(origin.EntriesList);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            return entries;
        }

        [XmlElement(ElementName = "Entry")]
        public List<StatisticsLogEntry> EntriesList
        {
            get { return _entriesList; }
            private set { _entriesList = value; }
        }

        protected string ConstructPath(DateTime fixationTime, string directoryPath = null)
        {
            return Path.Combine(directoryPath, string.Format("log_{0}.xml", DateTime.Now.ToString("yyyy-MM-dd")));
        }
    }
}