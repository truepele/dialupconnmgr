using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Common.Extenssions;

namespace dialupconnmgr
{
    public class ConnectionHistory : StatisticsLogBase
    {
        private ListCollectionView _history;
        private List<StatisticsLogEntry> _grouppedHistory;

        public ConnectionHistory(string directoryPath = null)
            : base(directoryPath)
        {
           
        }

        public async void LoadHistory(DateTime startDate, DateTime endDate)
        {
            EntriesList.Clear();
            List<StatisticsLogEntry> entries = new List<StatisticsLogEntry>();

            while (startDate.Date <= endDate.Date)
            {
                entries.AddRange(await ReadEntries(startDate.Date));
                startDate = startDate.AddDays(1);
            }

            if (entries.Any())
            {
                EntriesList.Clear();
                EntriesList = entries;
                GrouppedHistory = (from e in EntriesList
                    group e by e.UserName
                    into g
                    select g.Sum()).ToList();

                History = new ListCollectionView(EntriesList);
                History.GroupDescriptions.Add(new PropertyGroupDescription("UserName"));
            }
        }

        public List<StatisticsLogEntry> GrouppedHistory
        {
            get { return _grouppedHistory; }
            set
            {
                SetProperty(ref _grouppedHistory, value);
            }

        }

        public ListCollectionView History
        {
            get { return _history; }
            set { SetProperty(ref _history, value); }
        }
    }
}
