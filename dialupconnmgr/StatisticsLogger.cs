using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Xml;
using System.Xml.Serialization;
using DotRas;

namespace dialupconnmgr
{
    public class StatisticsLogger
    {
        private const int SaveInterval = 10;
        private List<StatisticsLogEntry> _entriesList = new List<StatisticsLogEntry>();
        SemaphoreSlim _semaphor = new SemaphoreSlim(1,1);
        private string _path;
        private string _directoryPath;
        private DateTime _lastSavedFixationTime;

        private XmlSerializer _serializer;
        private DateTime _lastWriteTime;

        public StatisticsLogger()
        {
        }

        public StatisticsLogger(DateTime date, string directoryPath = null)
        {
            _serializer = new XmlSerializer(typeof(StatisticsLogger));

            _directoryPath = !string.IsNullOrEmpty(directoryPath)?System.IO.Path.GetDirectoryName(directoryPath)
                :Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");

            _path = ConstructPath(date);
            if (System.IO.File.Exists(_path))
            {
                using (XmlReader reader = new XmlTextReader(_path))
                {
                    try
                    {
                        var origin = _serializer.Deserialize(reader) as StatisticsLogger;
                        if (origin != null)
                        {
                            _entriesList = new List<StatisticsLogEntry>(origin.EntriesList);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            
        }

        [XmlElement(ElementName = "Entry")]
        public List<StatisticsLogEntry> EntriesList
        {
            get { return _entriesList; }
            private set { _entriesList = value; }
        }

        private string ConstructPath(DateTime fixationTime)
        {

            return System.IO.Path.Combine(_directoryPath, string.Format("log_{0}.xml", DateTime.Now.ToString("yyyy-MM-dd")));
        }

        private void Init(DateTime fixationTime)
        {
            var path = ConstructPath(fixationTime);
            if (string.IsNullOrEmpty(_path) || string.Compare(path, _path, StringComparison.OrdinalIgnoreCase) != 0)
            {
                _path = path;
                if (!Directory.Exists(_directoryPath))
                {
                    Directory.CreateDirectory(_directoryPath);
                }
                _entriesList.Clear();
            }
        }

        public async Task Log(DateTime fixationTime, int rashandleHash, string username, string phonebookentryName, RasLinkStatistics statistics, double upspeed, double downspeed)
        {
            await _semaphor.WaitAsync();

            try
            {
                if (fixationTime.Date > _lastSavedFixationTime.Date)
                {
                    if (_lastSavedFixationTime > DateTime.MinValue)
                        Serialize();
                    _lastSavedFixationTime = fixationTime;
                    Init(_lastSavedFixationTime);
                }
                else if ((fixationTime - _lastSavedFixationTime).TotalSeconds > SaveInterval)
                {
                    Serialize();
                    _lastSavedFixationTime = fixationTime;
                }

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
            }
            catch (Exception e)
            {
                //Debugger.Break();
            }
            finally
            {
                _semaphor.Release();
            }


        }

        public async Task Save()
        {
            await _semaphor.WaitAsync();
            Serialize();
            _semaphor.Release();
        }

        private void Serialize()
        {
            using (var writer = new XmlTextWriter(_path, Encoding.UTF8))
            {
                _serializer.Serialize(writer, this);
            }
        }
    }
}
