using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using SEAppBuilder.Common;

namespace dialupconnmgr
{
    public class StatisticsLogBase : BindableBase, IXmlSerializable
    {
        protected List<StatisticsLogEntry> _entriesList = new List<StatisticsLogEntry>();
        protected static SemaphoreSlim _semaphor = new SemaphoreSlim(1, 1);//, "loggingSemaphore");
        protected string _directoryPath;
        private XmlSerializer _serializer;

        public StatisticsLogBase()
        {
        }

        public StatisticsLogBase(string directoryPath = null)
        {
            _serializer = new XmlSerializer(typeof(StatisticsLogBase), new XmlRootAttribute("StatisticsLog"));
            _directoryPath = !string.IsNullOrEmpty(directoryPath) ? Path.GetDirectoryName(directoryPath)
               : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log");
        }

        protected async Task<List<StatisticsLogEntry>> ReadEntries(DateTime date)
        {
            List<StatisticsLogEntry> entries = null;
            await _semaphor.WaitAsync();

            try
            {
                var path = ConstructPath(date, _directoryPath);
                if (File.Exists(path))
                {
                    using (XmlReader reader = new XmlTextReader(path))
                    {
                        try
                        {
                            var origin = _serializer.Deserialize(reader) as StatisticsLogBase;

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
            }
            finally
            {
                _semaphor.Release();
            }
            
            return entries;
        }
        
        public List<StatisticsLogEntry> EntriesList
        {
            get { return _entriesList; }
            set { SetProperty(ref _entriesList, value); }
        }

        protected string ConstructPath(DateTime fixationTime, string directoryPath = null)
        {
            return Path.Combine(directoryPath, string.Format("log_{0}.xml", fixationTime.Date.ToString("yyyy-MM-dd")));
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public virtual void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            if (!reader.IsEmptyElement)
            {
                EntriesList = new List<StatisticsLogEntry>();
                reader.ReadStartElement();

                reader.Read();
                while (!reader.EOF && reader.Name == "Entry")
                {
                    var s = new XmlSerializer(typeof(StatisticsLogEntry), new XmlRootAttribute("Entry"));
                    var e = s.Deserialize(reader) as StatisticsLogEntry;
                    if (e != null)
                    {
                        EntriesList.Add(e);
                    }
                }
                reader.ReadEndElement();
            }
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            var s = new XmlSerializer(typeof (StatisticsLogEntry), new XmlRootAttribute("Entry"));
            writer.WriteStartElement("Entries");
            foreach (var entry in EntriesList)
            {
                s.Serialize(writer, entry);
            }
            writer.WriteEndElement();
        }

        protected virtual void Serialize(XmlWriter writer)
        {
            _serializer.Serialize(writer, this);
        }
    }
}