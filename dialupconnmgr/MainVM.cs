using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DotRas;
using NvtlApiWrapper;
using SEAppBuilder.Common;
using wv;

namespace dialupconnmgr
{
    
    class MainVM:BindableBase
    {
        #region Constants

        private const int WatchingInterval = 1000;
        private const string Dayusername = "IT";
        private const string Daypassword = "IT";
        private const string Nightusername = "UNLIM_NIGHT";
        private const string Nightpassword = "UNLIM_NIGHT";

        #endregion

        #region _fields

        private RasConnectionWatcher _connwatcher;
        private readonly RasDialer _dialer = new RasDialer();
        private RasConnection _conn;
        private readonly SemaphoreSlim _dialingSemaphor = new SemaphoreSlim(1);
        private string _errorText;
        private string _stateText;
        private RasLinkStatistics _statistics;
        private RasConnectionState _connectionState;
        private readonly DateTime _nightbegintime = DateTime.MinValue.Date + new TimeSpan(0, 1, 0);
        private readonly DateTime _nightendtime = DateTime.MinValue.Date + new TimeSpan(8, 0, 0);
        private string _currentUsername;
        private string _currentPassword;
        private string _lastconnectedUsername;
        private double _signalStrength;
        private string _deviceName;
        private bool _isDeviceAttached;
        private double _bytesTransmitted;
        private double _bytesReceived;
        private TimeSpan _connectionDuration;
        private Timer _watchingTimer;
        private readonly ApiWrapper _nvtlApiWrapper = new ApiWrapper();
        private const int SpeedcalQueueCount = 5;
        private readonly Queue<StatisticsEntry> _speedcalcQueue = new Queue<StatisticsEntry>(SpeedcalQueueCount);
        private double _upSpeed;
        private double _downSpeed;
        private List<RasEntry> _rasEntries;
        private RasEntry _selectedRasEntrie;
        private ImageSource _appIcon;
        StatisticsLogger _logger;

        #endregion

        #region Commands

        public DelegateCommand ConnectDisconnectCommand { get; private set; }
        public DelegateCommand ShowCommand { get; set; }
        public DelegateCommand ExitCommand { get; set; }

        #endregion

        public MainVM()
        {
            var pbk = new RasPhoneBook();
            pbk.Open(RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User));
            RasEntries =
            (from e in pbk.Entries.ToList()
             //where e.Device != null && e.Device.DeviceType == RasDeviceType.Modem
                select e).ToList();

            InitCredentials();
            _lastconnectedUsername = CurrentUsername;

            if (!String.IsNullOrEmpty(EntryName))
            {
                SelectedRasEntry = (from e in RasEntries
                    where string.CompareOrdinal(e.Name, EntryName) == 0
                    select e).FirstOrDefault();
            }
            if (SelectedRasEntry == null)
                SelectedRasEntry = RasEntries.FirstOrDefault();

            ConnectDisconnectCommand = new DelegateCommand(ConnectDisconnectCommand_Executed);
            ExitCommand = new DelegateCommand(ExitCommand_Executed);


            if (Left == 0)
                Left = double.NaN;
            if (Top == 0)
                Top = double.NaN;

            InitAppIcon();
        }

        #region Public Methods

        public void StartWatcher()
        {
            InitConnectionWatcher();
            _watchingTimer = new Timer(o => Task.Factory.StartNew(WatchingProc), null, 0, WatchingInterval);
        }

        public async Task StopWatcher()
        {
            await _dialingSemaphor.WaitAsync();
            IsBusy = true;
            _watchingTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (_nvtlApiWrapper.IsApiLoaded() && _nvtlApiWrapper.IsInitialized())
            {
                try
                {
                    if (_nvtlApiWrapper.getIsDeviceAttached())
                        _nvtlApiWrapper.DetachDevice();
                }
                catch (Exception e)
                {
                    ErrorText = e.ToString();
                }
                finally
                {
                    _nvtlApiWrapper.ReleaseSession();
                }
            }

            _dialingSemaphor.Release();
            IsBusy = false;
        }

        public async override void Save()
        {
            if (_logger != null)
            {
                await _logger.Save();
            }
            base.Save();
        }

        #endregion

        #region private methods
        private async Task Connect()
        {
            ErrorText = string.Empty;
            await _dialingSemaphor.WaitAsync();
            IsBusy = true;
            try
            {
                ClearStatistic();
                _dialer.EntryName = EntryName;
                _dialer.PhoneBookPath = RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User);

                _dialer.Credentials = new NetworkCredential(CurrentUsername, CurrentPassword);
                _dialer.DialCompleted -= DialerOnDialCompleted;
                _dialer.DialCompleted += DialerOnDialCompleted;
                _dialer.StateChanged -= DialerOnStateChanged;
                _dialer.StateChanged += DialerOnStateChanged;
                _dialer.Error -= DialerOnError;
                _dialer.Error += DialerOnError;

                _dialer.DialAsync();
            }
            catch (Exception ex)
            {
                IsBusy = false;
                _dialingSemaphor.Release();
                ErrorText = ex.ToString();
            }
        }

        private async Task StopAndDisconnect()
        {
            await StopWatcher();
            await Disconnect();
        }

        private async Task Disconnect()
        {
            if (_conn != null)
            {
                await _dialingSemaphor.WaitAsync();
                IsBusy = true;

                try
                {
                    if (_conn != null)
                    {
                        await Task.Factory.StartNew(_conn.HangUp);
                        ConnectionState = RasConnectionState.Disconnected;
                    }

                }
                catch (Exception e)
                {
                    ErrorText = e.ToString();
                }
                finally
                {
                    IsBusy = false;
                    _dialingSemaphor.Release();
                }
            }
        }

        private async void WatchingProc()
        {
            try
            {
                HandleDevice(_nvtlApiWrapper);

                InitCredentials();

                if (!IsBusy && !string.IsNullOrEmpty(EntryName))
                {
                    if (_conn != null)
                    {
                        var connstatus = _conn.GetConnectionStatus();
                        if (connstatus.ConnectionState == RasConnectionState.Disconnected)
                        {
                            if (IsKeepConnection)
                                await Connect();
                        }
                        else
                        {
                            ConnectionState = connstatus.ConnectionState;
                            UpdateStatistic();

                            if (_lastconnectedUsername != CurrentUsername)
                            {
                                await Disconnect();
                                await Connect();
                                _lastconnectedUsername = CurrentUsername;
                            }
                        }
                    }
                    else
                    {
                        ConnectionState = RasConnectionState.Disconnected;
                        InitConnectionWatcher();

                        if (_conn == null)
                        {
                            if (IsKeepConnection)
                                await Connect();
                        }
                        else
                        {
                            ConnectionState = _conn.GetConnectionStatus().ConnectionState;
                        }

                    }
                }
            }
            catch (Exception)
            {
                InitConnectionWatcher();
            }

        }

        private void HandleDevice(ApiWrapper apiobj)
        {
            if (apiobj.IsApiLoaded() && !apiobj.IsInitialized())
            {
                if (!apiobj.Init())
                {
                    SignalStrength = 0;
                    DeviceName = "";
                }
            }
            else if (apiobj.IsInitialized())
            {
                if (!apiobj.getIsDeviceAttached())
                {
                    var devices = apiobj.GetAvailableDevices();
                    if (devices != null && devices.Any())
                    {
                        var device = devices.First();
                        if (apiobj.AttachDevice(device) == 0)
                        {
                            DeviceName = device.szFriendlyName;
                            IsDeviceAttached = true;
                        }
                    }
                }

                if (apiobj.getIsDeviceAttached())
                {
                    SignalStrength = (((double)apiobj.getSignalStrenght()) / 4) * 100;
                }
                else
                {
                    SignalStrength = 0;
                    DeviceName = "";
                    IsDeviceAttached = false;
                }
            }
        }

        private void UpdateStatistic()
        {
            if (_conn != null)
            {
                var fixationTime = DateTime.Now;
                Statistics = _conn.GetConnectionStatistics();
                if (Statistics != null)
                {
                    ConnectionDuration = Statistics.ConnectionDuration;
                    BytesReceived = Statistics.BytesReceived;
                    BytesTransmitted = Statistics.BytesTransmitted;
                    CalcSpeed(new StatisticsEntry
                    {
                        Duration = ConnectionDuration,
                        BytesReceived = Statistics.BytesReceived,
                        BytesTransmitted = Statistics.BytesTransmitted
                    });

                    Exception e;
                    if (_logger == null)
                    {
                        try
                        {
                            _logger = new StatisticsLogger(fixationTime);
                        }
                        catch (Exception ee)
                        {
                            Debugger.Break();
                        }
                        
                    }
                    _logger.Log(fixationTime, _conn.Handle.GetHashCode(), CurrentUsername, EntryName, Statistics, UpSpeed, DownSpeed);
                }
            }
        }

        private void ClearStatistic()
        {
            Statistics = null;
            ConnectionDuration = TimeSpan.Zero;
            BytesReceived = 0;
            BytesTransmitted = 0;
            _speedcalcQueue.Clear();
            UpSpeed = 0;
            DownSpeed = 0;
        }

        private void DialerOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            ErrorText = errorEventArgs.ToString();
        }

        private void DialerOnStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            ConnectionState = stateChangedEventArgs.State;
            if (!string.IsNullOrEmpty(stateChangedEventArgs.ErrorMessage))
                ErrorText = stateChangedEventArgs.ErrorMessage;
        }

        private void InitCredentials()
        {
            if (DateTime.Now.TimeOfDay >= _nightbegintime.TimeOfDay && DateTime.Now.TimeOfDay <= _nightendtime.TimeOfDay)
            {
                CurrentUsername = Nightusername;
                CurrentPassword = Nightpassword;
            }
            else
            {
                CurrentUsername = Dayusername;
                CurrentPassword = Daypassword;
            }
        }

        private void DialerOnDialCompleted(object sender, DialCompletedEventArgs dialCompletedEventArgs)
        {
            _dialer.DialCompleted -= DialerOnDialCompleted;

            if (dialCompletedEventArgs.Connected)
            {
                InitConnectionWatcher();
                UpdateStatistic();
                ErrorText = string.Empty;
            }
            else
            {
                ConnectionState = RasConnectionState.Disconnected;
            }

            IsBusy = false;
            _dialingSemaphor.Release();
        }

        private void InitConnectionWatcher()
        {
            try
            {
                _conn = GetActiveConnection(EntryName);

                if (_conn != null)
                {
                    if (_connwatcher == null)
                    {
                        _connwatcher = new RasConnectionWatcher();
                    }
                    else
                    {
                        _connwatcher.Disconnected -= ConnwatcherOnDisconnected;
                        _connwatcher.Error -= ConnwatcherOnError;
                    }
                    _connwatcher.Handle = _conn.Handle;
                    _connwatcher.Disconnected += ConnwatcherOnDisconnected;
                    _connwatcher.Error += ConnwatcherOnError;
                }
            }
            catch (Exception e)
            {
                ErrorText = e.ToString();
            }
        }

        private void ConnwatcherOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            ErrorText = errorEventArgs.ToString();
        }

        private RasConnection GetActiveConnection(string name = null, bool checkismodem = false)
        {
            return (from c in RasConnection.GetActiveConnections()
                    where
                        (string.IsNullOrEmpty(name) || c.EntryName == name) &&
                        (checkismodem == false || c.Device.DeviceType == RasDeviceType.Modem)
                    select c).FirstOrDefault();
        }

        private void ConnwatcherOnDisconnected(object sender, RasConnectionEventArgs rasConnectionEventArgs)
        {
            _connwatcher.Disconnected -= ConnwatcherOnDisconnected;
            if (IsKeepConnection)
                Connect();
        }

        private void CalcSpeed(StatisticsEntry statisticsEntry)
        {
            var beginningItem = new StatisticsEntry();
            if (_speedcalcQueue.Count == SpeedcalQueueCount - 1)
            {
                beginningItem = _speedcalcQueue.Dequeue();
            }
            else if (_speedcalcQueue.Count > 0)
            {
                beginningItem = _speedcalcQueue.Peek();
            }

            if (beginningItem.Duration.TotalMilliseconds > 0)
            {
                var interval = statisticsEntry.Duration - beginningItem.Duration;
                DownSpeed = ((statisticsEntry.BytesReceived - beginningItem.BytesReceived) /
                             (interval.TotalSeconds)) * 8;
                UpSpeed = ((statisticsEntry.BytesTransmitted - beginningItem.BytesTransmitted) /
                           (interval.TotalSeconds)) * 8;
            }

            _speedcalcQueue.Enqueue(statisticsEntry);
        }

        private async void ExitCommand_Executed(object o)
        {
            await StopWatcher();
            App.Current.Shutdown();
        }

        private void ConnectDisconnectCommand_Executed(object o)
        {
            if (ConnectionState == RasConnectionState.Disconnected)
            {
                IsKeepConnection = (new YesNoMessageBox("Keep connection alive?", Left, Top + 150).ShowDialog()) == true;

                Connect();
            }
            else if (ConnectionState == RasConnectionState.Connected)
            {
                IsKeepConnection = false;
                Disconnect();
            }
        }

        private void InitAppIcon()
        {
            if (IsDeviceAttached)
            {
                if (SignalStrength > 0 && SignalStrength <= 25)
                {
                    AppIcon = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/signal1.ico"));
                }
                else if (SignalStrength > 25 && SignalStrength <= 50)
                {
                    AppIcon = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/signal2.ico"));
                }
                else if (SignalStrength > 50  && SignalStrength <= 75)
                {
                    AppIcon = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/signal3.ico"));
                }
                else if (SignalStrength > 75)
                {
                    AppIcon = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/signal4.ico"));
                }
                else
                {
                    AppIcon = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/signal0.ico"));
                }
            }
            else
            {
                AppIcon = new BitmapImage(new Uri(@"pack://application:,,,/Resources/Images/signal_nodevice.ico"));
            }
        }

        #endregion

        #region Properties

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

        public ImageSource AppIcon
        {
            get { return _appIcon; }
            set { SetProperty(ref _appIcon, value); }
        }

        public List<RasEntry> RasEntries
        {
            get { return _rasEntries; }
            private set { SetProperty(ref _rasEntries, value); }
        }

        public RasEntry SelectedRasEntry
        {
            get { return _selectedRasEntrie; }
            set
            {
                if (SetProperty(ref _selectedRasEntrie, value) && value != null)
                {
                    if (EntryName != value.Name)
                    {
                        IsKeepConnection = (new YesNoMessageBox("Keep connection alive?", Left, Top + 150).ShowDialog()) ==
                                           true;
                        EntryName = value.Name;
                    }
                }
            }
        }

        public string ErrorText
        {
            get { return _errorText; }
            set { SetProperty(ref _errorText, value); }
        }

        public string StateText
        {
            get { return _stateText; }
            private set
            {
                SetPropertySync(ref _stateText, value);
            }
        }

        [UserScopedSetting]
        public string EntryName
        {
            get { return GetSettingProperty<string>(); }
            set
            {
                if (SetSettingProperty(value))
                {
                    Save();
                    _conn = null;
                }
            }
        }

        [UserScopedSetting]
        public bool IsKeepConnection
        {
            get { return GetSettingProperty<bool>(); }
            set
            {
                if (SetSettingProperty(value))
                {
                    Save();
                }
            }
        }

        public string CurrentUsername
        {
            get { return _currentUsername; }
            set { SetProperty(ref _currentUsername, value); }
        }

        public string CurrentPassword
        {
            get { return _currentPassword; }
            set { SetProperty(ref _currentPassword, value); }
        }

        public RasConnectionState ConnectionState
        {
            get { return _connectionState; }
            private set
            {
                if (SetProperty(ref _connectionState, value))
                {
                    StateText = value.ToString();
                    if (value == RasConnectionState.Connected)
                    {
                        ErrorText = string.Empty;
                    }
                }
            }
        }

        public double UpSpeed
        {
            get { return _upSpeed; }
            set { SetProperty(ref _upSpeed, value); }
        }

        public double DownSpeed
        {
            get { return _downSpeed; }
            set { SetProperty(ref _downSpeed, value); }
        }

        public RasLinkStatistics Statistics
        {
            get { return _statistics; }
            private set { SetProperty(ref _statistics, value); }
        }

        public double BytesTransmitted
        {
            get { return _bytesTransmitted; }
            set { SetProperty(ref _bytesTransmitted, value); }
        }

        public double BytesReceived
        {
            get { return _bytesReceived; }
            set { SetProperty(ref _bytesReceived, value); }
        }

        public TimeSpan ConnectionDuration
        {
            get { return _connectionDuration; }
            set { SetProperty(ref _connectionDuration, value); }
        }

        public double SignalStrength
        {
            get { return _signalStrength; }
            set
            {
                if (SetPropertySync(ref _signalStrength, value))
                {
                    App.DispatcherInvokeAsync(InitAppIcon);
                }
            }
        }

        public string DeviceName
        {
            get { return _deviceName; }
            set { SetProperty(ref _deviceName, value); }
        }

        public bool IsDeviceAttached
        {
            get { return _isDeviceAttached; }
            set
            {
                if (SetPropertySync(ref _isDeviceAttached, value))
                {
                    App.DispatcherInvokeAsync(InitAppIcon);
                }
            }
        }

        #endregion
    }
}
