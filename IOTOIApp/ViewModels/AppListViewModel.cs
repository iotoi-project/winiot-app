using System;

using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using IOTOIApp.Models;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using IOTOIApp.Services;
using Microsoft.Practices.ServiceLocation;

namespace IOTOIApp.ViewModels
{   
    public class AppListViewModel : ViewModelBase
    {
        public NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        public ObservableCollection<CmxApp> AppList { get; private set; } = new ObservableCollection<CmxApp>();

        public ObservableCollection<CmxAppPivot> PivotAppListItems { get; private set; } = new ObservableCollection<CmxAppPivot>();

        public ICommand AppLaunchCommand { get; private set; }
        public ICommand AppNavigateCommand { get; private set; }

        public AppListViewModel()
        {
            AppLaunchCommand = new RelayCommand<string>(AppLaunch);

            AppNavigateCommand = new RelayCommand<string>(AppNavigate);

            CreateAppList();

            AppList.Take(5).ToArray();
            AppList.Skip(5).ToArray();

            int PivotSize = 12;

            int j = 0;

            while(j <= AppList.Count)
            {
                var SplitAppList = new ObservableCollection<CmxApp>(AppList.Skip(j).Take(PivotSize));

                PivotAppListItems.Add(new CmxAppPivot
                {
                    Title = "pivot " + j,
                    PivotAppList = SplitAppList
                });

                j = (j == 0) ? PivotSize : (j * PivotSize);
            }
        }

        //test¿ë Applist
        private void CreateAppList()
        {
            AppList.Add(new CmxApp
            {
                AppName = "Plug",
                AppIconSvgUri = new Uri("ms-appx:///Assets/svg/plug-icon.svg"),
                AppLinkCommand = AppNavigateCommand,
                AppLinkParam = "IOTOIApp.ViewModels.Plug.PlugMainViewModel"
            });

            AppList.Add(new CmxApp
            {
                AppName = "Light",
                AppIcon = "\xEB50",
                AppLinkCommand = AppNavigateCommand,
                AppLinkParam = "IOTOIApp.ViewModels.Light.LightMainViewModel"
            });

            AppList.Add(new CmxApp
            {
                AppName = "Nest",
                AppIconSvgUri = new Uri("ms-appx:///Assets/svg/thermostat-icon.svg"),
                //AppLinkCommand = AppLaunchCommand,
                //AppLinkParam = "thermostat-launchmainpage://HostMainpage/Path1?param=This is param"
                AppLinkCommand = AppNavigateCommand,
                AppLinkParam = "IOTOIApp.ViewModels.Thermostat.ThermostatMainViewModel"
            });

            AppList.Add(new CmxApp
            {
                AppName = "Sensors",
                AppIcon = "\xE957",
                AppLinkCommand = AppNavigateCommand,
                AppLinkParam = "IOTOIApp.ViewModels.Sensor.SensorMainViewModel"
            });

            AppList.Add(new CmxApp
            {
                AppName = "CCTV",
                AppIcon = "\xE714",
                AppLinkCommand = AppNavigateCommand,
                AppLinkParam = "IOTOIApp.ViewModels.CCTV.CCTVMainViewModel"
            });

            AppList.Add(new CmxApp
            {
                AppName = "Settings",
                AppIcon = "\xE115",
                AppLinkCommand = AppNavigateCommand,
                AppLinkParam = "IOTOIApp.ViewModels.SettingsViewModel"
            });
            
        }

        public async void AppLaunch(string param)
        {
            Debug.WriteLine("AppLaunch!!!!! " + param);
            await LaunchAppAsync(param);
        }

        private void AppNavigate(string param)
        {
            Debug.WriteLine("AppNavi!!!!! " + param);
            var ShellVM = ServiceLocator.Current.GetInstance<ShellViewModel>();
            ShellVM.NaviToSettingPage(true);

            NavigationService.Navigate(param);
        }

        private async Task LaunchAppAsync(string uriStr)
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
    }
}
