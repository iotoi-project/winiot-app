using System;

using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.UI.Popups;
using IOTOIApp.Services;
using Microsoft.Practices.ServiceLocation;

namespace IOTOIApp.ViewModels.Sensor
{
    public class SensorMainViewModel : ViewModelBase
    {
        NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        public ICommand BackButtonClickedCommand { get; private set; }
        public ICommand GoSettingsPageCommand { get; private set; }
        public ICommand GoSensorDetailPageCommand { get; private set; }


        public SensorMainViewModel()
        {
            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);
            GoSettingsPageCommand = new RelayCommand(GoSettingsPage);
            GoSensorDetailPageCommand = new RelayCommand<object>(GoSensorDetailPage);
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

        void GoSettingsPage()
        {
            NavigationService.Navigate("IOTOIApp.ViewModels.Sensor.SensorSettingViewModel");
        }

        void GoSensorDetailPage(object tag)
        {
            NavigationService.Navigate("IOTOIApp.ViewModels.Sensor.SensorDetailViewModel", tag);
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
    }
}
