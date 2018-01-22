using IOTOIApp.ViewModels;
using IOTOIApp.ViewModels.Thermostat;
using IOTOIApp.Nest;
using IOTOIApp.Services;
using IOTOIApp.Utils;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IOTOIApp.Views.Thermostat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ThermostatMainPage : Page
    {
        DispatcherTimer SwitchTimer;
        DispatcherTimer ActionTimer;
        DispatcherTimer ActionSubTimer;

        ThermostatMainViewModel ViewModel
        {
            get { return DataContext as ThermostatMainViewModel; }
        }


        public ThermostatMainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //if (null == ViewModel.LoginButtonClickedCommand)
            //{
                ViewModel.LoginButtonClickedCommand = new RelayCommand<object>(Login);
            //}            

            if (ViewModel.InitDevice())
            {
                if (!string.IsNullOrEmpty(Global.Instance.AppParams))
                {
                    SwitchTimer = new DispatcherTimer();

                    SwitchTimer.Tick += SwitchTimer_Tick;
                    SwitchTimer.Interval = TimeSpan.FromMilliseconds(100);

                    SwitchTimer.Start();
                }
            }
        }

        private void SwitchTimer_Tick(object sender, object e)
        {
            SwitchTimer.Stop();

            if (Global.Instance.AppParams.Contains("target_temperature_f")) // && !ViewModel.OnTemperatureSetting)
            {
                FanCtrl.TemperatureView.IsChecked = true;
                //ViewModel.SwitchViewCommand.Execute(FanCtrl.TemperatureView);
            }
            else
            if (Global.Instance.AppParams.Contains("fan_timer_")) // && !ViewModel.OnFanSetting)
            {
                TemperatureCtrl.FanView.IsChecked = true;
                ViewModel.InitFanViewCommand.Execute(null);
                //ViewModel.SwitchViewCommand.Execute(TemperatureCtrl.FanView);
            }

            ActionTimer = new DispatcherTimer();

            ActionTimer.Tick += ActionTimer_Tick;
            ActionTimer.Interval = TimeSpan.FromMilliseconds(300);

            ActionTimer.Start();
        }


        private void ActionTimer_Tick(object sender, object e)
        {
            ActionTimer.Stop();

            if (Global.Instance.AppParams.Contains("target_temperature_f"))
            {
                string[] param = Global.Instance.AppParams.Split(new string[] { " " }, StringSplitOptions.None);

                ViewModel.DeviceList[0].target_temperature = Double.Parse(param[1]);
                return;
            }


            if (Global.Instance.AppParams.Contains("fan_timer_active"))
            {
                string[] param = Global.Instance.AppParams.Split(new string[] { " " }, StringSplitOptions.None);

                Models.FanDuration fd = ViewModel.DurationList.Where(D => D.Duration == param[1]).SingleOrDefault();

                FanCtrl.DuCarousel.SelectedItem = fd;

                ActionSubTimer = new DispatcherTimer();

                ActionSubTimer.Tick += ActionSubTimer_Tick;
                ActionSubTimer.Interval = TimeSpan.FromMilliseconds(100);

                ActionSubTimer.Start();                
                return;
            }


            if (Global.Instance.AppParams.Contains("fan_timer_stop"))
            {
                FanCtrl.StopFan.IsChecked = false;
                return;
            }
        }


        private void ActionSubTimer_Tick(object sender, object e)
        {
            ActionSubTimer.Stop();

            FanCtrl.StartFan.IsChecked = true;
        }


        private void AuthProcess_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            if (null != args.Uri && args.Uri.AbsolutePath.Contains("/login/oauth2/pincode/"))
            {
                ViewModel.SettingView = false;
                ViewModel.AuthProcessView = false;

                ThermostatAPI api = Global.Instance.ThermostatAPI;

                string json = args.Uri.AbsolutePath.Replace("/login/oauth2/pincode/", "");

                Task<bool> t = Task.Run(async () => await api.GetAccessToken(json));
                t.Wait();

                if (t.Result)
                {
                    //Task m = Task.Run(async () => await SetTokenToMainApp(api.AuthToken));
                    //m.Wait();

                    var message = new ValueSet();
                    message.Add("Type", "COMMON");
                    message.Add("Command", "SETNESTTOKEN");
                    message.Add("NESTTOKEN", api.AuthToken);

                    //AppServiceResponse response = await ConnectionService._connection.connection.SendMessageAsync(message);
                    IOTOI.Common.CommonService.GetReturnData(message);

                    ViewModel.InitDevice();
                }
                else
                {
                    ViewModel.Setting();
                }
            }
        }


        //async Task<bool> SetTokenToMainApp(string token)
        //{
        //    if (!await ConnectionService._connection.GetConnection()) return false;

        //    var message = new ValueSet();
        //    message.Add("Type", "COMMON");
        //    message.Add("Command", "SETNESTTOKEN");
        //    message.Add("NESTTOKEN", token);

        //    AppServiceResponse response = await ConnectionService._connection.connection.SendMessageAsync(message);

        //    return true;
        //}


        async void Login(object button)
        {
            (button as Button).Focus(FocusState.Pointer);

            if (string.IsNullOrEmpty(ViewModel.ProductID))
            {
                ViewModel.IDBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x41, 0x41));
                return;
            }

            if (string.IsNullOrEmpty(ViewModel.ProductSecret))
            {
                ViewModel.SecretBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x41, 0x41));
                return;
            }

            if (string.IsNullOrEmpty(ViewModel.AuthorizationURL) || false == ViewModel.AuthorizationURL.Contains("https://home.nest.com/login/oauth2?client_id="))
            {
                ViewModel.URLBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0x41, 0x41));
                return;
            }

            Global.Instance.SetAuthValue(ViewModel.ProductID, ViewModel.ProductSecret, ViewModel.AuthorizationURL);

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["ProductID"] = ViewModel.ProductID;
            localSettings.Values["ProductSecret"] = ViewModel.ProductSecret;
            localSettings.Values["AuthorizationURL"] = ViewModel.AuthorizationURL;

            await Windows.UI.Xaml.Controls.WebView.ClearTemporaryWebDataAsync();

            ViewModel.SettingView = false;
            ViewModel.AuthProcessView = true;

            AuthProcess.Navigate(new Uri(Global.Instance.AuthorizationURL));
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Global.Instance.TemperatureCheckTimer = new Timer(CheckTemperatureCallback,
                                                    null,
                                                    (int)TimeSpan.FromMilliseconds(1000).TotalMilliseconds,
                                                    (int)TimeSpan.FromMilliseconds(500).TotalMilliseconds);
        }



        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (null != Global.Instance.TemperatureCheckTimer)
            {
                Global.Instance.TemperatureCheckTimer.Dispose();
            }
        }


        async void CheckTemperatureCallback(object state)
        {
            // do some work not connected with UI
            DateTime Current = DateTime.Now;

            ThermostatDevice target = null;
            double target_temperature = -1;
            string temperature_scale = "";

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    // do some work on UI here;
                    if (0 < ViewModel.DeviceList.Count)
                    {
                        for (int i = 0; i < ViewModel.DeviceList.Count; ++i)
                        {
                            if (ViewModel.DeviceList[i].ChangeTemperature)
                            {
                                if (2000 <= (Current - ViewModel.DeviceList[i].LatestdChangeTime).TotalMilliseconds)
                                {
                                    ViewModel.DeviceList[i].ChangeTemperature = false;

                                    target = ViewModel.DeviceList[i].Reference;
                                    target_temperature = ViewModel.DeviceList[i].target_temperature;
                                    temperature_scale = ViewModel.DeviceList[i].temperature_scale;
                                    break;
                                }
                            }
                        }
                    }
                });

            if (null == target)
            {
                return;
            }

            ThermostatAPI api = Global.Instance.ThermostatAPI;

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string token = localSettings.Values["NestAccessToken"] as string;

            string t = api.SetTemperature(target, target_temperature, temperature_scale).Result;

            //Task<string> t = Task.Run(async () => await api.SetTemperature(ViewModel.DeviceList[target].Reference, ViewModel.DeviceList[target].target_temperature, ViewModel.DeviceList[target].temperature_scale));
            //t.Wait();

            if (t.Contains("error"))
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    var dialog = new MessageDialog(t);
                    dialog.ShowAsync();

                    if (api.AuthorizationError)
                    {
                        ViewModel.ToSettingButtonClickedCommand.Execute(null);
                    }
                });
            }
        }
    }
}
