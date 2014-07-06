using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for YesNoMessageBox.xaml
    /// </summary>
    public partial class YesNoMessageBox : Window
    {
        public YesNoMessageBox(string text, double left, double top)
        {
            InitializeComponent();
            TextTB.Text = text;
            WindowStartupLocation = WindowStartupLocation.Manual;

            Left = left;
            Top = top;
        }

        private void ButtonYes_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonNo_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
