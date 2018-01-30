using System;

using GalaSoft.MvvmLight;
using Windows.Devices.WiFi;
using System.Diagnostics;
using IOTOIApp.Utils;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Core;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using IOTOIApp.Services;
using Microsoft.Practices.ServiceLocation;
using Windows.Foundation.Collections;
using IOTOI.Model.Db;
using System.Linq;

namespace IOTOIApp.ViewModels
{
    public class FooterViewModel : ViewModelBase
    {
        private static NetworkPresenter networkPresenter = new NetworkPresenter();
        private static WiFiAvailableNetwork ConnectedNetwork;

        private byte? _wifiSymbol;
        public byte? WifiSymbol
        {
            get { return _wifiSymbol; }
            set { Set(ref _wifiSymbol, value); }
        }

        private SolidColorBrush _wfiSymbolColor;
        public SolidColorBrush WifiSymbolColor
        {
            get { return _wfiSymbolColor; }
            set { Set(ref _wfiSymbolColor, value); }
        }

        private SolidColorBrush _zgbeeSymbolColor = DeactivatedSymbolColor;
        public SolidColorBrush ZigbeeSymbolColor
        {
            get { return _zgbeeSymbolColor; }
            set { Set(ref _zgbeeSymbolColor, value); }
        }

        private SolidColorBrush _cCTVSymbolColor = DeactivatedSymbolColor;
        public SolidColorBrush CCTVSymbolColor
        {
            get { return _cCTVSymbolColor; }
            set { Set(ref _cCTVSymbolColor, value); }
        }

        private static SolidColorBrush ActivatedSymbolColor = new SolidColorBrush(Windows.UI.Colors.Cyan);
        private static SolidColorBrush DeactivatedSymbolColor = new SolidColorBrush(Windows.UI.Colors.Gray);

        private Visibility _rightPanelVisibility = Visibility.Visible;
        public Visibility RightPanelVisibility
        {
            get { return _rightPanelVisibility; }
            set { Set(ref _rightPanelVisibility, value); }
        }

        private Visibility _backButtonVisibility = Visibility.Collapsed;
        public Visibility BackButtonVisibility
        {
            get { return _backButtonVisibility; }
            set { Set(ref _backButtonVisibility, value); }
        }

        public ICommand BackButtonClickedCommand { get; private set; }

        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        public FooterViewModel()
        {
            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);

            NetworkInformation.NetworkStatusChanged += NetworkInformationOnNetworkStatusChanged;
            CheckInternetAccess();
            CheckCCTVStreaming();
            //CheckZigbeeAccess();
        }

        public async void CheckZigbeeAccess()
        {
            try
            {
                var message = new ValueSet();
                message.Add("Type", "ZigBee");
                message.Add("Command", "ZigBeeStatus");

                var rtnMessage = new ValueSet();
                rtnMessage = IOTOI.Common.CommonService.GetReturnData(message);
                int ZigBeeErrorCode = Convert.ToInt32(rtnMessage["Result"]);
                Debug.WriteLine("CheckZigbeeAccess ZigBeeErrorCode : " + ZigBeeErrorCode);

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ZigbeeSymbolColor = (ZigBeeErrorCode == 0) ? ActivatedSymbolColor : DeactivatedSymbolColor;
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message, "CheckZigbeeAccess Exception : ");
            }


        }

        private async void WifiSymbolChange(bool IsNetworkAvailable, byte? SignalBar)
        {
            Debug.WriteLine("SignalBar :: " + SignalBar);
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (IsNetworkAvailable) 
                {
                    WifiSymbolColor = ActivatedSymbolColor;
                    WifiSymbol = SignalBar ?? 0;
                } else
                {
                    WifiSymbolColor = DeactivatedSymbolColor;
                    WifiSymbol = 5;
                }
            });
        }

        private bool _isNetworkAvailable;
        public event Action<bool> OnNetworkAvailabilityChange = delegate { };

        public bool IsNetworkAvailable
        {
            get
            {
                return _isNetworkAvailable;
            }
            protected set
            {
                if (_isNetworkAvailable == value) return;
                _isNetworkAvailable = value;
                OnNetworkAvailabilityChange(value);
            }
        }

        private void CheckInternetAccess()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            IsNetworkAvailable = (connectionProfile != null &&
                                 connectionProfile.GetNetworkConnectivityLevel() ==
                                 NetworkConnectivityLevel.InternetAccess);
            Debug.WriteLine("has network changed: " + IsNetworkAvailable);

            byte? SignalBar = (connectionProfile == null) ? 0 : connectionProfile.GetSignalBars();
            WifiSymbolChange(IsNetworkAvailable, SignalBar);
        }

        private void NetworkInformationOnNetworkStatusChanged(object sender)
        {
            CheckInternetAccess();
            Debug.WriteLine("network status changed");
        }

        public void CheckCCTVStreaming()
        {
            Debug.WriteLine("CheckCCTVStreaming Start");
            CCTVSymbolColor = DeactivatedSymbolColor;
            using (var db = new Context())
            {
                foreach(IOTOI.Model.CCTV cctv in db.CCTV.ToList()){
                    if(!String.IsNullOrEmpty(cctv.CCTVType)) {
                        CCTVSymbolColor = ActivatedSymbolColor;
                        Debug.WriteLine("CCTVStreaming Activated");
                        break;
                    }
                }
            }
            Debug.WriteLine("CheckCCTVStreaming End");
        }

        private void BackButtonClicked()
        {
            if (NavigationService.CanGoBack)
            {
                var ShellVM = ServiceLocator.Current.GetInstance<ShellViewModel>();
                ShellVM.NaviToSettingPage(false);
                NavigationService.GoBack();
            }
        }

    }
}
