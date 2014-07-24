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
using Common.Extenssions;
using DotRas;

namespace dialupconnmgr
{
    public class StatisticsLogger : StatisticsLogBase
    {
        private const int SaveInterval = 5;
        private DateTime _lastSavedFixationTime;
        private string _path;

        public StatisticsLogger() : base()
        {
        }

        public StatisticsLogger(DateTime date, string directoryPath = null):base(directoryPath)
        {
            _path = ConstructPath(date, _directoryPath);
            _entriesList = ReadEntries(date);
        }

        public async Task LogDisconnection(DateTime fixationTime)
        {
            await _semaphor.WaitAsync();
            try
            {
                if (_entriesList.Any())
                {
                    _entriesList.Last().SetDisconnected(fixationTime);
                    Serialize();
                }
            }
            finally
            {
                _semaphor.Release();
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

        protected void Init(DateTime fixationTime)
        {
            var path = ConstructPath(fixationTime, _directoryPath);
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
    }
}
