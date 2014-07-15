using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DotRas;
using NvtlApiWrapper;
using SEAppBuilder.Common;

namespace dialupconnmgr
{
    class Watcher : BindableBase
    {
        private RasConnectionWatcher _connwatcher;
        RasDialer _dialer = new RasDialer();
        private RasConnection _conn;
        private int _watchingInterval = 1000;
        SemaphoreSlim _dialingSemaphor = new SemaphoreSlim(1);
        private string _errorText;
        private string _stateText;
        private RasLinkStatistics _statistics;
        private CancellationTokenSource _cancellationTokenSource;
        private RasConnectionState _connectionState;
        private string _dayusername = "IT";
        private string _daypassword = "IT";
        private string _nigthusername = "UNLIM_NIGHT";
        private string _nightpassword = "UNLIM_NIGHT";
        private DateTime _nightbegintime = DateTime.MinValue.Date + new TimeSpan(0, 1,0);
        private DateTime _nightendtime = DateTime.MinValue.Date + new TimeSpan(8, 0, 0);
        private string _currentUsername;
        private string _currentPassword;
        private string _lastconnectedUsername;
        private double _signalStrength;
        private bool tmp;
        private string _deviceName;
        private bool _isDeviceAttached;
        private double _bytesTransmitted;
        private double _bytesReceived;
        private TimeSpan _connectionDuration;
        private Timer _watchingTimer;
        ApiWrapper _nvtlApiWrapper = new ApiWrapper();

        public Watcher(string entryName = null)
            :base(true)
        {
            if (entryName != null)
                EntryName = entryName;
            InitCredentials();
            _lastconnectedUsername = CurrentUsername;
        }

        public async void Start()
        {
            InitWatcher();
            _watchingTimer = new Timer(o => Task.Factory.StartNew(WatcherProc), null, 0, _watchingInterval);
        }

        private async void WatcherProc()
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
                                Connect();
                        }
                        else
                        {
                            ConnectionState = connstatus.ConnectionState;
                            UpdateStatistic();

                            if (_lastconnectedUsername != CurrentUsername)
                            {
                                await Disconnect();
                                Connect();
                                _lastconnectedUsername = CurrentUsername;
                            }
                        }
                    }
                    else
                    {
                        ConnectionState = RasConnectionState.Disconnected;
                        InitWatcher();

                        if (_conn == null)
                        {
                            if (IsKeepConnection)
                                Connect();
                        }
                        else
                        {
                            ConnectionState = _conn.GetConnectionStatus().ConnectionState;
                        }

                    }
                }
            }
            catch (Exception e)
            {
                InitWatcher();
            }

        }

        private void HandleDevice(NvtlApiWrapper.ApiWrapper apiobj)
        {
            if (apiobj.IsApiLoaded() && !apiobj.IsInitialized())
            {
                if (!apiobj.Init())
                {
                    SignalStrength = 0;
                    DeviceName = "";
                    return;
                }
            }

            var devicename = "";

            if (apiobj.IsInitialized())
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
                    SignalStrength = (((double)apiobj.getSignalStrenght())/4)*100;                    
                }
                else
                {
                    SignalStrength = 0;
                    DeviceName = "";
                    IsDeviceAttached = false;
                }
            }
        }

        public async void Stop()
        {
            await _dialingSemaphor.WaitAsync();
            IsBusy = true;

            _dialingSemaphor.Release();
            IsBusy = false;

            try
            {
                if (_nvtlApiWrapper.IsApiLoaded() && _nvtlApiWrapper.IsInitialized())
                {
                    if (_nvtlApiWrapper.getIsDeviceAttached())
                        _nvtlApiWrapper.DetachDevice();

                    _nvtlApiWrapper.ReleaseSession();
                }
            }
            catch
            {
            }

            _watchingTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void UpdateStatistic()
        {
            Statistics = _conn.GetConnectionStatistics();
        }

        public RasLinkStatistics Statistics
        {
            get { return _statistics; }
            private set
            {
                if (SetProperty(ref _statistics, value))
                {
                    if (Statistics != null)
                    ConnectionDuration = Statistics.ConnectionDuration;
                    BytesReceived = (double)Statistics.BytesReceived/(1024*1024);
                    BytesTransmitted = (double)Statistics.BytesTransmitted / (1024 * 1024);
                }
            }
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
            get
            {
                return _signalStrength; 
            }
            set { SetPropertySync(ref _signalStrength, value); }
        }

        public string DeviceName
        {
            get { return _deviceName; }
            set { SetProperty(ref _deviceName, value); }
        }

        public bool IsDeviceAttached
        {
            get
            {
                return _isDeviceAttached; 
            }
            set { SetPropertySync(ref _isDeviceAttached, value); }
        }

        private void ClearStatistic()
        {
            Statistics = null;
        }

        public async void Connect()
        {
            ErrorText = "";
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

        public async void StopAndDisconnect()
        {
            Stop();
            await Disconnect();
        }

        public async Task Disconnect()
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

        private void DialerOnError(object sender, ErrorEventArgs errorEventArgs)
        {
            ErrorText = errorEventArgs.ToString();
        }

        public string ErrorText
        {
            get { return _errorText; }
            set
            {
                SetProperty(ref _errorText, value);
            }
        }

        private void DialerOnStateChanged(object sender, StateChangedEventArgs stateChangedEventArgs)
        {
            ConnectionState = stateChangedEventArgs.State;
            if (!string.IsNullOrEmpty(stateChangedEventArgs.ErrorMessage))
                ErrorText = stateChangedEventArgs.ErrorMessage;
        }

        public string StateText
        {
            get { return _stateText; }
            private set
            {
                if (SetPropertySync(ref _stateText, value) && value == "Connected")
                {
                    ErrorText = "";
                }
            }
        }

        [UserScopedSetting]
        public string EntryName
        {
            get
            {
                return GetSettingProperty<string>();
            }
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

        private void InitCredentials()
        {
            if (DateTime.Now.TimeOfDay >= _nightbegintime.TimeOfDay && DateTime.Now.TimeOfDay <= _nightendtime.TimeOfDay)
            {
                CurrentUsername = _nigthusername;
                CurrentPassword = _nightpassword;
            }
            else
            {
                CurrentUsername = _dayusername;
                CurrentPassword = _daypassword;
            }
        }

        private void DialerOnDialCompleted(object sender, DialCompletedEventArgs dialCompletedEventArgs)
        {
            _dialer.DialCompleted -= DialerOnDialCompleted;
            if (dialCompletedEventArgs.Connected)
            {
                InitWatcher();
                UpdateStatistic();
                ErrorText = "";
            }
            else
            {
                ConnectionState = RasConnectionState.Disconnected;
            }

            IsBusy = false;
            _dialingSemaphor.Release();
        }

        private async void InitWatcher()
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
                    (string.IsNullOrEmpty(name) || c.EntryName == name) && (checkismodem == false || c.Device.DeviceType == RasDeviceType.Modem)
                select c).FirstOrDefault();
        }

        private void ConnwatcherOnDisconnected(object sender, RasConnectionEventArgs rasConnectionEventArgs)
        {
            _connwatcher.Disconnected -= ConnwatcherOnDisconnected;
            if (IsKeepConnection)
                Connect();
        }

        public RasConnectionState ConnectionState
        {
            get { return _connectionState; }
            private set
            {
                if (SetProperty(ref _connectionState, value))
                {
                    StateText = value.ToString();
                }
            }
        }
    }
}
