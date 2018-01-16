using IOTOIApp.Models;
using IOTOIApp.Nest;
using IOTOIApp.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace IOTOIApp.ViewModels.Thermostat
{
    public class ThermostatMainViewModel : ViewModelBase
    {
        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }


        ObservableCollection<FanDuration> _durationList;
        public ObservableCollection<FanDuration> DurationList
        {
            get { return _durationList; }
            private set { Set(ref _durationList, value); }
        }

        ObservableCollection<Models.Thermostat> _deviceList;
        public ObservableCollection<Models.Thermostat> DeviceList
        {
            get { return _deviceList; }
            private set { Set(ref _deviceList, value); }
        }

        bool _onFanSetting;
        public bool OnFanSetting
        {
            get { return _onFanSetting; }
            set { Set(ref _onFanSetting, value); }
        }

        bool _onTemperatureSetting;
        public bool OnTemperatureSetting
        {
            get { return _onTemperatureSetting; }
            set { Set(ref _onTemperatureSetting, value); }
        }

        bool _settingView;
        public bool SettingView
        {
            get { return _settingView; }
            set { Set(ref _settingView, value); }
        }

        bool _authProcessView;
        public bool AuthProcessView
        {
            get { return _authProcessView; }
            set { Set(ref _authProcessView, value); }
        }

        string _productID;
        public string ProductID
        {
            get { return _productID; }
            set { Set(ref _productID, value); }
        }

        string _productSecret;
        public string ProductSecret
        {
            get { return _productSecret; }
            set { Set(ref _productSecret, value); }
        }

        string _authorizationURL;
        public string AuthorizationURL
        {
            get { return _authorizationURL; }
            set { Set(ref _authorizationURL, value); }
        }

        SolidColorBrush _iDBrush;
        public SolidColorBrush IDBrush
        {
            get { return _iDBrush; }
            set { Set(ref _iDBrush, value); }
        }

        SolidColorBrush _secretBrush;
        public SolidColorBrush SecretBrush
        {
            get { return _secretBrush; }
            set { Set(ref _secretBrush, value); }
        }

        SolidColorBrush _uRLBrush;
        public SolidColorBrush URLBrush
        {
            get { return _uRLBrush; }
            set { Set(ref _uRLBrush, value); }
        }

        public ICommand BackButtonClickedCommand { get; private set; }
        public ICommand BackSettingButtonClickedCommand { get; private set; }
        public ICommand ToMainButtonClickedCommand { get; private set; }
        public ICommand ToSettingButtonClickedCommand { get; private set; }
        public ICommand LoginButtonClickedCommand { get; set; }
        public ICommand SwitchViewCommand { get; private set; }

        public ICommand HideFlipViewButtonCommand { get; set; }
        public ICommand TemperatureUpCommand { get; private set; }
        public ICommand TemperatureDownCommand { get; private set; }

        public ICommand InitFanViewCommand { get; set; }


        public ThermostatMainViewModel()
        {
            DurationList = new ObservableCollection<FanDuration>()
            {
                new FanDuration() { Duration = "15" },
                new FanDuration() { Duration = "30" },
                new FanDuration() { Duration = "45" },
                new FanDuration() { Duration = "60" },
                new FanDuration() { Duration = "120" },
                new FanDuration() { Duration = "240" },
                new FanDuration() { Duration = "480" },
                new FanDuration() { Duration = "720" }
            };

            DeviceList = new ObservableCollection<Models.Thermostat>();

            IDBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x7a, 0x7b, 0x7b));
            SecretBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x7a, 0x7b, 0x7b));
            URLBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x7a, 0x7b, 0x7b));

            OnTemperatureSetting = false;
            OnFanSetting = false;

            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);
            BackSettingButtonClickedCommand = new RelayCommand(BackSettingButtonClicked);

            ToMainButtonClickedCommand = new RelayCommand(CheckToBack);
            ToSettingButtonClickedCommand = new RelayCommand(Setting);

            SwitchViewCommand = new RelayCommand<object>(SwitchView);

            TemperatureUpCommand = new RelayCommand<object>(TemperatureUp);
            TemperatureDownCommand = new RelayCommand<object>(TemperatureDown);
        }


        void BackButtonClicked()
        {
            if (NavigationService.CanGoBack)
            {
                var ShellVM = ServiceLocator.Current.GetInstance<ShellViewModel>();
                ShellVM.NaviToSettingPage(false);
                NavigationService.GoBack();
            }
        }

        void BackSettingButtonClicked()
        {
            SettingView = true;
            AuthProcessView = false;
        }

        async Task LaunchAppAsync(string uriStr)
        {
            Uri uri = new Uri(uriStr);
            var promptOptions = new Windows.System.LauncherOptions();
            promptOptions.TreatAsUntrusted = false;

            bool isSuccess = await Windows.System.Launcher.LaunchUriAsync(uri, promptOptions);

            if (!isSuccess)
            {
                string msg = "Launch failed";
                await new MessageDialog(msg).ShowAsync();
            }
        }


        void CheckToBack()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string token = localSettings.Values["NestAccessToken"] as string;

            if(string.IsNullOrEmpty(token))
            {
                BackButtonClicked();
            }
            else
            {
                InitDevice();
            }
        }

        bool firstInit = true;
        public bool InitDevice()
        {
            SettingView = false;
            AuthProcessView = false;

            ThermostatAPI api = Global.Instance.ThermostatAPI;

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string token = localSettings.Values["NestAccessToken"] as string;

            if (string.IsNullOrEmpty(token))
            {
                Setting();

                return false;
            }
            else
            {
                api.ApplyAccessToken(token);

                Task<bool> t = Task.Run(async () => await api.GetDevices());
                t.Wait();

                if (t.Result)
                {
                    int count = DeviceList.Count;
                    if (0 == count)
                    {
                        foreach (ThermostatDevice d in api.ThermostatDevices)
                        {
                            FanDuration nfd = DurationList.Where(L => L.Duration == d.fan_timer_duration.ToString()).SingleOrDefault();

                            Models.Thermostat nts = new Models.Thermostat(d);
                            nts.Duration = nfd;
                            nts.RollDuration = nfd;

                            nts.Tick = 1; //("C" == nts.temperature_scale) ? 1 : 2;

                            nts.FanRun = nts.fan_timer_active == true;
                            nts.FanStop = nts.fan_timer_active == false;

                            if (d == api.ThermostatDevices.First())
                            {
                                nts.FanBrush = new SolidColorBrush(Colors.White);
                                nts.FanBallBrush = new SolidColorBrush(Colors.White);

                                nts.Duration.Brush = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xcb, 0x00));
                                nts.Duration.BallBrush = new SolidColorBrush(Color.FromArgb(0xff, 0xff, 0xcb, 0x00));
                            }

                            DeviceList.Add(nts);
                        }
                    }
                    else
                    {
                        if (api.ThermostatDevices.Count < count)
                        {
                            DeviceList.Clear();
                        }

                        foreach (ThermostatDevice d in api.ThermostatDevices)
                        {
                            FanDuration nfd = DurationList.Where(L => L.Duration == d.fan_timer_duration.ToString()).SingleOrDefault();

                            Models.Thermostat tm = DeviceList.Where(T => T.device_id == d.device_id).SingleOrDefault();
                            if (null == tm || string.IsNullOrEmpty(tm.device_id))
                            {
                                Models.Thermostat nts = new Models.Thermostat(d);
                                nts.Duration = nfd;
                                nts.RollDuration = nfd;

                                nts.Tick = 1; //("C" == nts.temperature_scale) ? 1 : 2;

                                nts.FanRun = nts.fan_timer_active == true;
                                nts.FanStop = nts.fan_timer_active == false;

                                DeviceList.Add(nts);
                                continue;
                            }

                            if (null != tm || false == string.IsNullOrEmpty(tm.device_id))
                            {
                                tm.Update(d, nfd);
                            }
                        }
                    }

                    if (firstInit)
                    {
                        firstInit = false;
                        OnTemperatureSetting = true;
                    }

                    return true;
                }
                else
                {
                    Setting();
                }

                return false;
            }
        }


        public void Setting()
        {
            SettingView = true;
            AuthProcessView = false;

            IDBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x7a, 0x7b, 0x7b));
            SecretBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x7a, 0x7b, 0x7b));
            URLBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x7a, 0x7b, 0x7b));

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            ProductID = localSettings.Values["ProductID"] as string;
            ProductSecret = localSettings.Values["ProductSecret"] as string;
            AuthorizationURL = localSettings.Values["AuthorizationURL"] as string;

            //
            // 임시값. 나중에 제거 요망
            //
            if (string.IsNullOrEmpty(ProductID))
            {
                ProductID = "a8ceca3b-a9a7-4ba3-a227-1bb6467fc845";
            }

            if (string.IsNullOrEmpty(ProductSecret))
            {
                ProductSecret = "u08oxnVJQI3PXGnnDjryFeVmT";
            }

            if (string.IsNullOrEmpty(AuthorizationURL))
            {
                AuthorizationURL = "https://home.nest.com/login/oauth2?client_id=a8ceca3b-a9a7-4ba3-a227-1bb6467fc845&state=STATE";
            }
            //
            //
            //
        }


        bool fan_first = true;
        void SwitchView(object view)
        {
            RadioButton target = view as RadioButton;

            target.IsChecked = true;

            OnFanSetting = ("temperature" == target.Name);
            OnTemperatureSetting = !OnFanSetting;

            if (0 < DeviceList.Count)
            {
                if (OnFanSetting) // && fan_first)
                {
                    fan_first = false;
                    InitFanViewCommand.Execute(null);
                }

                if (OnTemperatureSetting)
                {
                    HideFlipViewButtonCommand.Execute(null);
                }
            }
        }


        void TemperatureUp(object device)
        {
            Models.Thermostat target = device as Models.Thermostat;

            if ("C" == target.temperature_scale)
            {
                if (target.target_temperature + 0.5 >= target.set_temperature_max)
                {
                    target.target_temperature = target.set_temperature_max;
                }
                else
                {
                    target.target_temperature += 0.5;
                }
            }
            else
            {
                if (target.target_temperature + 1 >= target.set_temperature_max)
                {
                    target.target_temperature = target.set_temperature_max;
                }
                else
                {
                    target.target_temperature++;
                }
            }
        }


        void TemperatureDown(object device)
        {
            Models.Thermostat target = device as Models.Thermostat;

            if ("C" == target.temperature_scale)
            {
                if (target.target_temperature - 0.5 <= target.set_temperature_min)
                {
                    target.target_temperature = target.set_temperature_min;
                }
                else
                {
                    target.target_temperature -= 0.5; ;
                }
            }
            else
            {
                if (target.target_temperature - 1 <= target.set_temperature_min)
                {
                    target.target_temperature = target.set_temperature_min;
                }
                else
                {
                    target.target_temperature--;
                }
            }
        }
    }
}
