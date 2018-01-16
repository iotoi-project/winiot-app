using IOTOIApp.ViewModels.Thermostat;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class Temperature : UserControl
    {
        public RadioButton FanView { get { return fan; } }


        ThermostatMainViewModel ViewModel
        {
            get { return DataContext as ThermostatMainViewModel; }
        }


        public Temperature()
        {
            this.InitializeComponent();

            this.Loaded += Temperature_Loaded;
        }

        private void Temperature_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Temperature_Loaded;

            ViewModel.HideFlipViewButtonCommand = new RelayCommand(HideFlipViewButtons);

            HideFlipViewButtons();
        }

        


        private void DeviceCarousel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Models.Thermostat target = DeviceCarousel.SelectedItem as Models.Thermostat;
            if (null == target)
            {
                return;
            }
            
            for (int i = 0; i < ViewModel.DeviceList.Count; ++i)
            {
                ViewModel.DeviceList[i].DeviceBrush = (target == ViewModel.DeviceList[i]) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87));
                ViewModel.DeviceList[i].DeviceBallBrush = (target == ViewModel.DeviceList[i]) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Transparent);

                ViewModel.DeviceList[i].DevicePrevBallBrush = (target == ViewModel.DeviceList[i] && 0 < i && 1 < ViewModel.DeviceList.Count) ? 
                                                              new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87)) : new SolidColorBrush(Colors.Transparent);

                ViewModel.DeviceList[i].DeviceNextBallBrush = (target == ViewModel.DeviceList[i] && (i + 1) < ViewModel.DeviceList.Count) ?
                                                              new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87)) : new SolidColorBrush(Colors.Transparent);


            }

            FlipView.Visibility = (target.is_online) ? Visibility.Visible : Visibility.Collapsed;
            offline.Visibility = (false == target.is_online) ? Visibility.Visible : Visibility.Collapsed;
            hDown.Visibility = (target.is_online) ? Visibility.Visible : Visibility.Collapsed;
            hUp.Visibility = (target.is_online) ? Visibility.Visible : Visibility.Collapsed;
            //tSlider.Visibility = (target.is_online) ? Visibility.Visible : Visibility.Collapsed;

            FlipView.Focus(FocusState.Pointer);
        }


        void ButtonHide(FlipView f, string name)
        {
            Button b;
            b = FindVisualChild<Button>(f, name);
            if (null != b)
            {
                b.Opacity = 0.0;
                b.IsHitTestVisible = false;
            }
        }


        void HideFlipViewButtons()
        {
            ButtonHide(FlipView, "PreviousButtonHorizontal");
            ButtonHide(FlipView, "NextButtonHorizontal");
            ButtonHide(FlipView, "PreviousButtonVertical");
            ButtonHide(FlipView, "NextButtonVertical");
        }


        childItemType FindVisualChild<childItemType>(DependencyObject obj, string name) where childItemType : FrameworkElement
        {
            // Exec
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is childItemType && ((FrameworkElement)child).Name == name)
                    return (childItemType)child;
                else
                {
                    childItemType childOfChild = FindVisualChild<childItemType>(child, name);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
