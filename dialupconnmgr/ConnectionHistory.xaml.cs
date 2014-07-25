using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace dialupconnmgr
{
    /// <summary>
    /// Interaction logic for ConnectionHistory.xaml
    /// </summary>
    public partial class ConnectionHistoryView : Window
    {
        private MainVM _vm;
        public ConnectionHistoryView(MainVM vm)
        {
            DataContext =_vm = vm;
            InitializeComponent();
        }
    }
}
