using IOTOIApp.Models;
using IOTOIApp.Nest;
using IOTOIApp.ViewModels.Thermostat;
using GalaSoft.MvvmLight.Command;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
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
    public sealed partial class Fan : UserControl
    {
        public RadioButton TemperatureView { get { return temperature; } }

        public Carousel DuCarousel { get { return DurationCarousel; } }

        public RadioButton StartFan { get { return startFan; } }
        public RadioButton StopFan { get { return stopFan; } }


        ThermostatMainViewModel ViewModel
        {
            get { return DataContext as ThermostatMainViewModel; }
        }


        public Fan()
        {
            this.InitializeComponent();

            this.Loaded += Fan_Loaded;
        }

        private void Fan_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Fan_Loaded;

            ViewModel.InitFanViewCommand = new RelayCommand(InitFanView);
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
                ViewModel.DeviceList[i].FanBrush = (target == ViewModel.DeviceList[i]) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87));
                ViewModel.DeviceList[i].FanBallBrush = (target == ViewModel.DeviceList[i]) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Transparent);

                ViewModel.DeviceList[i].FanPrevBallBrush = (target == ViewModel.DeviceList[i] && 0 < i && 1 < ViewModel.DeviceList.Count) ?
                                                           new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87)) : new SolidColorBrush(Colors.Transparent);

                ViewModel.DeviceList[i].FanNextBallBrush = (target == ViewModel.DeviceList[i] && (i + 1) < ViewModel.DeviceList.Count) ?
                                                           new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87)) : new SolidColorBrush(Colors.Transparent);
            }

            DurationCarousel.Visibility = target.has_fan ? Visibility.Visible : Visibility.Collapsed;

            stopFan.Visibility = target.has_fan ? Visibility.Visible : Visibility.Collapsed;
            startFan.Visibility = target.has_fan ? Visibility.Visible : Visibility.Collapsed;
            if (target.is_online)
            {
                offline.Visibility = Visibility.Collapsed;
                noFan.Visibility = target.has_fan ? Visibility.Collapsed : Visibility.Visible;                
            }
            else
            {
                offline.Visibility = Visibility.Visible;
                noFan.Visibility = Visibility.Collapsed;
            }
            

            if (target.has_fan)
            {
                DurationCarousel.SelectionChanged -= DurationCarousel_SelectionChanged;

                DurationCarousel.SelectedItem = target.Duration;

                for (int i = 0; i < ViewModel.DurationList.Count; ++i)
                {
                    ViewModel.DurationList[i].Brush = (target.Duration == ViewModel.DurationList[i]) ? new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xcb, 0x00)) : new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f));
                    ViewModel.DurationList[i].BallBrush = (target.Duration == ViewModel.DurationList[i]) ? new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xcb, 0x00)) : new SolidColorBrush(Colors.Transparent);

                    ViewModel.DurationList[i].PrevBallBrush = (target.Duration == ViewModel.DurationList[i] && 0 < i && 1 < ViewModel.DurationList.Count) ?
                                                              new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f)) : new SolidColorBrush(Colors.Transparent);

                    ViewModel.DurationList[i].NextBallBrush = (target.Duration == ViewModel.DurationList[i] && (i + 1) < ViewModel.DurationList.Count) ?
                                                              new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f)) : new SolidColorBrush(Colors.Transparent);
                }

                stopFan.IsChecked = target.FanStop;
                startFan.IsChecked = target.FanRun;

                DurationCarousel.SelectionChanged += DurationCarousel_SelectionChanged;
            }
        }

        private void DurationCarousel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FanDuration target = DurationCarousel.SelectedItem as FanDuration;
            if (null == target)
                return;

            Models.Thermostat tm = DeviceCarousel.SelectedItem as Models.Thermostat;
            if (null == tm)
                return;

            tm.RollDuration = target;

            for (int i = 0; i < ViewModel.DurationList.Count; ++i)
            {
                ViewModel.DurationList[i].Brush = (target == ViewModel.DurationList[i]) ? new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xcb, 0x00)) : new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f));
                ViewModel.DurationList[i].BallBrush = (target == ViewModel.DurationList[i]) ? new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xcb, 0x00)) : new SolidColorBrush(Colors.Transparent);

                ViewModel.DurationList[i].PrevBallBrush = (target == ViewModel.DurationList[i] && 0 < i && 1 < ViewModel.DurationList.Count) ?
                                                          new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f)) : new SolidColorBrush(Colors.Transparent);

                ViewModel.DurationList[i].NextBallBrush = (target == ViewModel.DurationList[i] && (i + 1) < ViewModel.DurationList.Count) ?
                                                          new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f)) : new SolidColorBrush(Colors.Transparent);
            }
        }


        void InitFanView()
        {
            DeviceCarousel.SelectedIndex = 0;

            DurationCarousel.SelectionChanged += DurationCarousel_SelectionChanged;

            if (0 < ViewModel.DeviceList.Count)
            {
                for (int f = 0; f < ViewModel.DeviceList.Count; ++f)
                {
                    ViewModel.DeviceList[f].FanBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87));
                    ViewModel.DeviceList[f].FanBallBrush = new SolidColorBrush(Colors.Transparent);

                    ViewModel.DeviceList[f].FanPrevBallBrush = new SolidColorBrush(Colors.Transparent);
                    ViewModel.DeviceList[f].FanNextBallBrush = new SolidColorBrush(Colors.Transparent);

                    for (int i = 0; i < ViewModel.DurationList.Count; ++i)
                    {
                        ViewModel.DurationList[i].Brush = new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f));
                        ViewModel.DurationList[i].BallBrush = new SolidColorBrush(Colors.Transparent);

                        ViewModel.DurationList[i].PrevBallBrush = new SolidColorBrush(Colors.Transparent);
                        ViewModel.DurationList[i].NextBallBrush = new SolidColorBrush(Colors.Transparent);
                    }

                    if (ViewModel.DeviceList[f].has_fan)
                    {
                        stopFan.IsChecked = ViewModel.DeviceList[f].FanStop;
                        startFan.IsChecked = ViewModel.DeviceList[f].FanRun;

                        for (int i = 0; i < ViewModel.DurationList.Count; ++i)
                        {
                            if (ViewModel.DeviceList[f].Duration == ViewModel.DurationList[i])
                            {
                                ViewModel.DurationList[i].Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xcb, 0x00));
                                ViewModel.DurationList[i].BallBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xcb, 0x00));

                                ViewModel.DurationList[i].PrevBallBrush = (0 < i && 1 < ViewModel.DurationList.Count) ?
                                                                          new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f)) : new SolidColorBrush(Colors.Transparent);

                                ViewModel.DurationList[i].NextBallBrush = ((i + 1) < ViewModel.DurationList.Count) ?
                                                                          new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f)) : new SolidColorBrush(Colors.Transparent);

                                DurationCarousel.SelectedIndex = i;
                                break;
                            }
                        }

                        break;
                    }
                }

                ViewModel.DeviceList[0].FanBrush = new SolidColorBrush(Colors.White);
                ViewModel.DeviceList[0].FanBallBrush = new SolidColorBrush(Colors.White);

                ViewModel.DeviceList[0].FanPrevBallBrush = new SolidColorBrush(Colors.Transparent);

                ViewModel.DeviceList[0].FanNextBallBrush = (1 < ViewModel.DeviceList.Count) ?
                                                           new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87)) : new SolidColorBrush(Colors.Transparent);

                if (!ViewModel.DeviceList[0].is_online)
                {
                    DurationCarousel.Visibility = Visibility.Collapsed;

                    stopFan.Visibility = Visibility.Collapsed;
                    startFan.Visibility = Visibility.Collapsed;

                    offline.Visibility = Visibility.Visible;
                    noFan.Visibility = Visibility.Collapsed;
                }
                else if (!ViewModel.DeviceList[0].has_fan)
                {
                    DurationCarousel.Visibility = Visibility.Collapsed;

                    stopFan.Visibility = Visibility.Collapsed;
                    startFan.Visibility = Visibility.Collapsed;

                    offline.Visibility = Visibility.Collapsed;
                    noFan.Visibility = Visibility.Visible;
                }
            }
        }

        private async void stopFan_Checked(object sender, RoutedEventArgs e)
        {
            Models.Thermostat target = DeviceCarousel.SelectedItem as Models.Thermostat;
            if (null == target)
            {
                return;
            }
            target.FanStop = stopFan.IsChecked.Value;

            if (target.fan_timer_active)
            {
                ThermostatAPI api = Global.Instance.ThermostatAPI;

                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                string token = localSettings.Values["NestAccessToken"] as string;

                Task<string> t = Task.Run(async () => await api.SetFanOff(target.Reference));
                t.Wait();

                if (t.Result.Contains("error"))
                {
                    var dialog = new MessageDialog(api.ErrorMessage);
                    await dialog.ShowAsync();

                    if (api.AuthorizationError)
                    {
                        ViewModel.ToSettingButtonClickedCommand.Execute(null);
                    }
                }
                else
                {
                    target.fan_timer_active = false;
                }
            }
        }

        private async void startFan_Checked(object sender, RoutedEventArgs e)
        {
            Models.Thermostat target = DeviceCarousel.SelectedItem as Models.Thermostat;
            if (null == target)
            {
                return;
            }

            target.Duration = target.RollDuration;
            target.FanRun = startFan.IsChecked.Value;

            if (!target.fan_timer_active)
            {
                ThermostatAPI api = Global.Instance.ThermostatAPI;

                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                string token = localSettings.Values["NestAccessToken"] as string;

                Task<string> t = Task.Run(async () => await api.SetFanTimer(target.Reference, int.Parse(target.Duration.Duration)));
                t.Wait();

                if (t.Result.Contains("error"))
                {
                    var dialog = new MessageDialog(api.ErrorMessage);
                    await dialog.ShowAsync();

                    if (api.AuthorizationError)
                    {
                        ViewModel.ToSettingButtonClickedCommand.Execute(null);
                    }
                }
                else
                {
                    target.fan_timer_active = true;
                }
            }
        }
    }
}
