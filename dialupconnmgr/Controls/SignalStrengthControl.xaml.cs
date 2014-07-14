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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace dialupconnmgr.Controls
{
    /// <summary>
    /// Interaction logic for SignalStrengthControl.xaml
    /// </summary>
    public partial class SignalStrengthControl : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty LevelActiveColorProperty = DependencyProperty.Register(
            "LevelActiveColor", typeof (SolidColorBrush), typeof (SignalStrengthControl), new PropertyMetadata(new SolidColorBrush(Colors.Green)));

        public static readonly DependencyProperty LevelInactiveColorProperty = DependencyProperty.Register(
            "LevelInactiveColor", typeof(SolidColorBrush), typeof(SignalStrengthControl), new PropertyMetadata(new SolidColorBrush(new Color(){A = 0x35, R = 0, G = 0x80, B = 00})));

        public static readonly DependencyProperty DeviceDetachedColorProperty = DependencyProperty.Register(
            "DeviceDetachedColor", typeof(SolidColorBrush), typeof(SignalStrengthControl), new PropertyMetadata(new SolidColorBrush(new Color() { A = 0x35, R = 0x9F, G = 00, B = 00 })));

        public SolidColorBrush DeviceDetachedColor
        {
            get { return (SolidColorBrush) GetValue(DeviceDetachedColorProperty); }
            set { SetValue(DeviceDetachedColorProperty, value); }
        }

        public static readonly DependencyProperty SignalStrengthProperty = DependencyProperty.Register(
            "SignalStrength", typeof(double), typeof(SignalStrengthControl), new PropertyMetadata(default(double), SignalStrengthChangedCallback));

        public static readonly DependencyProperty IsDeviceAttachedProperty = DependencyProperty.Register(
            "IsDeviceAttached", typeof(bool), typeof(SignalStrengthControl), new PropertyMetadata(default(bool), IsDeviceAttachedChangedCallback));

        public bool IsDeviceAttached
        {
            get { return (bool) GetValue(IsDeviceAttachedProperty); }
            set { SetValue(IsDeviceAttachedProperty, value); }
        }
        public double SignalStrength
        {
            get { return (double) GetValue(SignalStrengthProperty); }
            set
            {
                SetValue(SignalStrengthProperty, value);
            }
        }

        public SolidColorBrush LevelInactiveColor
        {
            get { return (SolidColorBrush) GetValue(LevelInactiveColorProperty); }
            set { SetValue(LevelInactiveColorProperty, value); }
        }

        public SolidColorBrush LevelActiveColor
        {
            get { return (SolidColorBrush) GetValue(LevelActiveColorProperty); }
            set { SetValue(LevelActiveColorProperty, value); }
        }
        #endregion


        public SignalStrengthControl()
        {
            InitializeComponent();
            Reinit();
        }

        private static void SignalStrengthChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((SignalStrengthControl)dependencyObject).Reinit();
        }

        private static void IsDeviceAttachedChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((SignalStrengthControl)dependencyObject).Reinit();
        }

        private void Reinit()
        {
            SolidColorBrush level1Brush, level2Brush, level3Brush, level4Brush;
            level1Brush = level2Brush = level3Brush = level4Brush = IsDeviceAttached? LevelInactiveColor : DeviceDetachedColor;

            if (IsDeviceAttached)
            {
                if (SignalStrength > 0)
                {
                    level1Brush = LevelActiveColor;
                }
                if (SignalStrength > 25)
                {
                    level2Brush = LevelActiveColor;
                }
                if (SignalStrength > 50)
                {
                    level3Brush = LevelActiveColor;
                }
                if (SignalStrength > 75)
                {
                    level4Brush = LevelActiveColor;
                }
            }

            Level1Border.Background = level1Brush;
            Level2Border.Background = level2Brush;
            Level3Border.Background = level3Brush;
            Level4Border.Background = level4Brush;
        }
        
    }
}
