using IOTOIApp.Control;
using IOTOIApp.Services;
using IOTOIApp.ViewModels.CCTV;
using IOTOIApp.ViewModels.Light;
using IOTOIApp.ViewModels.Plug;
using IOTOIApp.ViewModels.Sensor;
using IOTOIApp.ViewModels.Thermostat;
using IOTOIApp.Views;
using IOTOIApp.Views.CCTV;
using IOTOIApp.Views.Light;
using IOTOIApp.Views.Plug;
using IOTOIApp.Views.Sensor;
using IOTOIApp.Views.Thermostat;
using GalaSoft.MvvmLight.Ioc;

using Microsoft.Practices.ServiceLocation;

namespace IOTOIApp.ViewModels
{
    public class ViewModelLocator
    {
        NavigationServiceEx _navigationService = new NavigationServiceEx();

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register(() => _navigationService);
            SimpleIoc.Default.Register<ShellViewModel>();
            Register<MainViewModel, MainPage>();
            Register<HeaderViewModel, HeaderPage>();
            Register<RightPanelViewModel, RightPanelPage>();
            Register<SettingsViewModel, SettingsPage>();
            Register<NotificationViewModel, NotificationPage>();
            //Register<CortanaViewModel, CortanaPage>();
            Register<AppListViewModel, AppListPage>();
            Register<PowerViewModel, PowerPage>();
            Register<SettingNetworkViewModel, SettingNetwork>();
            Register<SettingBluetoothViewModel, SettingBluetooth>();
            Register<SettingZigbeeViewModel, SettingZigbee>();
            Register<SettingZWaveViewModel, SettingZWave>();
            Register<FooterViewModel, FooterPage>();

            Register<LightMainViewModel, LightMainPage>();
            Register<LightSettingViewModel, LightSettingPage>();

            Register<CCTVMainViewModel, CCTVMainPage>();
            Register<CCTVSettingViewModel, CCTVSettingPage>();
            Register<CCTVListViewModel, CCTVList>();

            Register<PlugMainViewModel, PlugMainPage>();
            Register<PlugSettingViewModel, PlugSettingPage>();

            Register<SensorMainViewModel, SensorMainPage>();
            Register<SensorSettingViewModel, SensorSettingPage>();
            Register<SensorDetailViewModel, SensorDetailPage>();

            Register<ThermostatMainViewModel, ThermostatMainPage>();
        }

        public PowerViewModel PowerViewModel => ServiceLocator.Current.GetInstance<PowerViewModel>();

        public AppListViewModel AppListViewModel => ServiceLocator.Current.GetInstance<AppListViewModel>();

        //public CortanaViewModel CortanaViewModel => ServiceLocator.Current.GetInstance<CortanaViewModel>();

        public NotificationViewModel NotificationViewModel => ServiceLocator.Current.GetInstance<NotificationViewModel>();

        public SettingsViewModel SettingsViewModel => ServiceLocator.Current.GetInstance<SettingsViewModel>();

        public HeaderViewModel HeaderViewModel => ServiceLocator.Current.GetInstance<HeaderViewModel>();

        public RightPanelViewModel RightPanelViewModel => ServiceLocator.Current.GetInstance<RightPanelViewModel>();

        public MainViewModel MainViewModel => ServiceLocator.Current.GetInstance<MainViewModel>();

        public ShellViewModel ShellViewModel => ServiceLocator.Current.GetInstance<ShellViewModel>();

        public SettingNetworkViewModel SettingNetworkViewModel => ServiceLocator.Current.GetInstance<SettingNetworkViewModel>();

        public SettingBluetoothViewModel SettingBluetoothViewModel => ServiceLocator.Current.GetInstance<SettingBluetoothViewModel>();

        public SettingZigbeeViewModel SettingZigbeeViewModel => ServiceLocator.Current.GetInstance<SettingZigbeeViewModel>();

        public SettingZWaveViewModel SettingZWaveViewModel => ServiceLocator.Current.GetInstance<SettingZWaveViewModel>();

        public FooterViewModel FooterViewModel => ServiceLocator.Current.GetInstance<FooterViewModel>();


        public LightMainViewModel LightMainViewModel => ServiceLocator.Current.GetInstance<LightMainViewModel>();
        public LightSettingViewModel LightSettingViewModel => ServiceLocator.Current.GetInstance<LightSettingViewModel>();

        public CCTVSettingViewModel CCTVSettingViewModel => ServiceLocator.Current.GetInstance<CCTVSettingViewModel>();
        public CCTVMainViewModel CCTVMainViewModel => ServiceLocator.Current.GetInstance<CCTVMainViewModel>();
        public CCTVListViewModel CCTVListViewModel => ServiceLocator.Current.GetInstance<CCTVListViewModel>();

        public PlugMainViewModel PlugMainViewModel => ServiceLocator.Current.GetInstance<PlugMainViewModel>();
        public PlugSettingViewModel PlugSettingViewModel => ServiceLocator.Current.GetInstance<PlugSettingViewModel>();

        public SensorMainViewModel SensorMainViewModel => ServiceLocator.Current.GetInstance<SensorMainViewModel>();
        public SensorSettingViewModel SensorSettingViewModel => ServiceLocator.Current.GetInstance<SensorSettingViewModel>();
        public SensorDetailViewModel SensorDetailViewModel => ServiceLocator.Current.GetInstance<SensorDetailViewModel>();

        public ThermostatMainViewModel ThermostatMainViewModel => ServiceLocator.Current.GetInstance<ThermostatMainViewModel>();

        public void Register<VM, V>() where VM : class
        {
            SimpleIoc.Default.Register<VM>();
            
            _navigationService.Configure(typeof(VM).FullName, typeof(V));
        }
    }
}
