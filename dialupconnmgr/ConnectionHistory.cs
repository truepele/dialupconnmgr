using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Extenssions;

namespace dialupconnmgr
{
    class ConnectionHistory : StatisticsLogBase
    {
        private List<StatisticsLogEntry> _history;

        public ConnectionHistory(string directoryPath = null)
            : base(directoryPath)
        {
           
        }

        public async void LoadHistory(DateTime startDate, DateTime endDate)
        {
            _entriesList.Clear();

            while (startDate.Date <= endDate.Date)
            {
                _entriesList.AddRange(await ReadEntries(startDate.Date));
                startDate = startDate.AddDays(1);
            }

            if (_entriesList.Any())
            {
                GroupedHistory = (from e in EntriesList
                    group e by e.UserName
                    into g
                    select g.Sum()).ToList();
            }
        }

        public List<StatisticsLogEntry> GroupedHistory
        {
            get { return _history; }
            set
            {
                //SetProperty(ref _history, value);
                _history = value;
            }

        }
    }
}
