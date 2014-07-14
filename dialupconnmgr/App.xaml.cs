using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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

        public App()
        {
            this.InitializeComponent();
            _mainWindow = new MainWindow();
            _mainWindow.Show();
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

