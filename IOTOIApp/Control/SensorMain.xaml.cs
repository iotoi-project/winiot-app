using IOTOIApp.ViewModels.Sensor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace IOTOIApp.Control
{
    public sealed partial class SensorMain : UserControl
    {
        SensorMainViewModel ViewModel
        {
            get { return DataContext as SensorMainViewModel; }
        }

        public SensorMain()
        {
            this.InitializeComponent();

            this.Loaded += SensorMain_Loaded;
        }

        private void SensorMain_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= SensorMain_Loaded;


            Title.Text = this.Tag.ToString();

            switch (Title.Text)
            {
                case "Motion Sensor":
                    MotionIcon.Visibility = Visibility.Visible;
                    break;
                case "Magnetic":
                    DoorIcon.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
