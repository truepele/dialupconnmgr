using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DotRas;
using wv;

namespace dialupconnmgr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainVM _model;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = _model = new MainVM();
           Loaded += OnLoaded;
           Closing += OnClosing;
           _model.ShowHideCommand = new DelegateCommand(ShowHideCommand_Execute);
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;

            await Task.Factory.StartNew(() => _model.Watcher.Start());
        }

        private void ShowHideCommand_Execute(object o)
        {
            if (this.Visibility == Visibility.Hidden)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            _model.Save();
            Hide();
            cancelEventArgs.Cancel = true;
        }
    }
}
