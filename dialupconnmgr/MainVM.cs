using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DotRas;
using SEAppBuilder.Common;
using wv;

namespace dialupconnmgr
{
    
    class MainVM:BindableBase
    {
        private Watcher _watcher;
        private List<RasEntry> _rasEntries;
        private RasEntry _selectedRasEntrie;
        public DelegateCommand ConnectDisconnectCommand { get; private set; }
        public DelegateCommand ShowHideCommand { get; set; }
        public DelegateCommand ExitCommand { get; set; }

        public MainVM()
        {
            //Watcher.Statistics.BytesTransmitted

            RasPhoneBook pbk = new RasPhoneBook();
            pbk.Open(RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User));
            RasEntries = (from e in pbk.Entries.ToList()
               // where e.Device.DeviceType == RasDeviceType.Modem
                select e).ToList();

            Watcher = new Watcher();
            if (!String.IsNullOrEmpty(Watcher.EntryName))
            {
                SelectedRasEntrie = (from e in RasEntries
                    where string.CompareOrdinal(e.Name, Watcher.EntryName) == 0
                    select e).FirstOrDefault();
            }
            if (SelectedRasEntrie == null)
                SelectedRasEntrie = RasEntries.FirstOrDefault();

            ConnectDisconnectCommand = new DelegateCommand(ConnectDisconnectCommand_Executed);
            ExitCommand = new DelegateCommand(ExitCommand_Executed);


            if (Left == 0)
                Left = double.NaN;
            if (Top == 0)
                Top = double.NaN;
        }

        private void ExitCommand_Executed(object o)
        {
            Watcher.Stop();
            App.Current.Shutdown();
        }

        private void ConnectDisconnectCommand_Executed(object o)
        {
            if (Watcher.ConnectionState == RasConnectionState.Disconnected)
            {
                Watcher.IsKeepConnection = (new YesNoMessageBox("Keep connection alive?", Left, Top + 150).ShowDialog()) == true;
                
                Watcher.Connect();
            }
            else if(Watcher.ConnectionState == RasConnectionState.Connected)
            {
                Watcher.IsKeepConnection = false;
                Watcher.Disconnect();
            }
        }

        public Watcher Watcher
        {
            get { return _watcher; }
            private set { SetProperty(ref _watcher, value); }
        }

        public List<RasEntry> RasEntries
        {
            get { return _rasEntries; }
            private set { SetProperty(ref _rasEntries, value); }
        }

        public RasEntry SelectedRasEntrie
        {
            get { return _selectedRasEntrie; }
            set
            {
                if (SetProperty(ref _selectedRasEntrie, value) && Watcher != null)
                {
                    if (Watcher.EntryName != value.Name)
                    {
                        Watcher.IsKeepConnection = (new YesNoMessageBox("Keep connection alive?", Left, Top+150).ShowDialog()) == true;
                        Watcher.EntryName = value.Name;
                    }

                }
            }
        }

        [UserScopedSetting]
        public double Left
        {
            get
            {
                return GetSettingProperty<double>();
            }
            set { SetSettingProperty(value); }
        }

        [UserScopedSetting]
        public double Top
        {
            get
            {
                return GetSettingProperty<double>();
            }
            set { SetSettingProperty(value); }
        }
    }
}
