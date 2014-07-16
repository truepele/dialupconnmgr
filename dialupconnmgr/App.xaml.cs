using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using NvtlApiWrapper;

namespace dialupconnmgr
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Window _mainWindow;
        static Mutex _singleInstMutex;
        private static EventWaitHandle _singleInstWaithandle;
        static bool _isMutexCreated;
        private static string _m_UID;
        private static string _w_UID;

        public App()
        {
            const string idword = "dialupconnmgrsynchronisation";
            _m_UID = Environment.UserName + idword+ "MUTEX";
            _w_UID = Environment.UserName + idword + "EventWaitHandle";

            _singleInstMutex = new Mutex(true, _m_UID, out _isMutexCreated);
            _singleInstWaithandle = new EventWaitHandle(false, EventResetMode.AutoReset, _w_UID);
            if (_isMutexCreated)
            {
                this.InitializeComponent();
                _mainWindow = new MainWindow();
                _mainWindow.Show();

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (_singleInstWaithandle.WaitOne() && _mainWindow != null)
                        {
                            await DispatcherInvokeAsync(() =>
                            {
                                _mainWindow.Show();
                                _mainWindow.Activate();
                            });
                        }
                    }
                });
            }
            else
            {
                _singleInstWaithandle.Set();
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_isMutexCreated && _singleInstMutex != null)
            {
                _singleInstMutex.ReleaseMutex();
                _singleInstMutex.Dispose();
                base.OnExit(e);
            }
        }

        public static async Task DispatcherInvokeAsync(Action action)
        {
            await _mainWindow.Dispatcher.InvokeAsync(action, DispatcherPriority.Send);
        }

        private void OnStartup(object sender, StartupEventArgs startupEventArgs)
        {
            //ApiWrapper api = new ApiWrapper();
            //api.Init();

            //if (api.IsApiLoaded())
            //{
            //    var devices = api.GetAvailableDevices();
            //    //api.DeviceDataReceived += ApiOnDeviceDataReceived;
            //    if (devices != null)
            //    {
            //        MessageBox.Show(string.Format("Found {0} devices", devices.Length));
            //        var firstdevice = devices.FirstOrDefault();

            //        if (firstdevice != null)
            //        {
            //            MessageBox.Show(string.Format("Device found: {0}", firstdevice.szFriendlyName));

            //            var attachresult = api.AttachDevice(firstdevice);
            //            MessageBox.Show(string.Format("attachresult: {0} ", attachresult));

            //        }
            //    }
        }


    }
}

