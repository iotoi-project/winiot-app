using System;

using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Notifications;
using RavinduL.LocalNotifications;
using RavinduL.LocalNotifications.Presenters;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using IOTOIApp.Services;
using IOTOIApp.Models;
using IOTOI.Model.ZigBee;

namespace IOTOIApp.ViewModels.Light
{
    public class LightSettingViewModel : ViewModelBase
    {
        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        private ObservableCollection<ZigBeeEndDevice> _lightDeviceListSources;
        public ObservableCollection<ZigBeeEndDevice> LightDeviceListSources
        {
            get { return _lightDeviceListSources; }
            set { Set(ref _lightDeviceListSources, value); }
        }

        private ZigBeeEndDevice _lightDeviceSelectedItem;
        public ZigBeeEndDevice LightDeviceSelectedItem
        {
            get { return _lightDeviceSelectedItem; }
            set { Set(ref _lightDeviceSelectedItem, value); }
        }

        private Visibility _saveButtonVisibility;
        public Visibility SaveButtonVisibility
        {
            get { return _saveButtonVisibility; }
            set { Set(ref _saveButtonVisibility, value); }
        }


        public ICommand BackButtonClickedCommand { get; private set; }

        public ICommand LightSelectionChangedCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }

        public LightSettingViewModel()
        {
            LightDeviceListSources = ZigbeeDeviceService.ZigbeeDeviceListSources;
            SaveButtonVisibility = (LightDeviceListSources.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

            TimeSpan period = TimeSpan.FromSeconds(2);
            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (LightDeviceListSources.Count == 0 ||
                        LightDeviceListSources.Count < ZigbeeDeviceService.ZigbeeDeviceListSources.Count)
                    {
                        LightDeviceListSources = ZigbeeDeviceService.ZigbeeDeviceListSources;
                        SaveButtonVisibility = (LightDeviceListSources.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
                    }
                });
            }, period);

            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);

            LightSelectionChangedCommand = new RelayCommand<ZigBeeEndDevice>(LightSelectionChanged);

            SaveCommand = new RelayCommand(Save);
        }

        private void BackButtonClicked()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void LightSelectionChanged(ZigBeeEndDevice lightDevice)
        {
            LightDeviceSelectedItem = lightDevice;
        }

        private void Save()
        {
            Debug.WriteLine("Save!!");
            ZigbeeDeviceService.SetEndDevices(LightDeviceListSources);
        }
    }
}
