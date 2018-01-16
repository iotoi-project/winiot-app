using System;

using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using IOTOIApp.Services;
using System.Collections.ObjectModel;
using IOTOI.Model.ZigBee;
using Windows.UI.Xaml;
using Windows.System.Threading;
using Windows.UI.Core;
using System.Diagnostics;

namespace IOTOIApp.ViewModels.Plug
{
    public class PlugSettingViewModel : ViewModelBase
    {
        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        private ObservableCollection<ZigBeeEndDevice> _plugDeviceListSources;
        public ObservableCollection<ZigBeeEndDevice> PlugDeviceListSources
        {
            get { return _plugDeviceListSources; }
            set { Set(ref _plugDeviceListSources, value); }
        }

        private ZigBeeEndDevice _plugDeviceSelectedItem;
        public ZigBeeEndDevice PlugDeviceSelectedItem
        {
            get { return _plugDeviceSelectedItem; }
            set { Set(ref _plugDeviceSelectedItem, value); }
        }

        private Visibility _saveButtonVisibility;
        public Visibility SaveButtonVisibility
        {
            get { return _saveButtonVisibility; }
            set { Set(ref _saveButtonVisibility, value); }
        }

        public ICommand BackButtonClickedCommand { get; private set; }
        public ICommand PlugSelectionChangedCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public PlugSettingViewModel()
        {
            PlugDeviceListSources = ZigbeeDeviceService.ZigbeeDeviceListSources;
            SaveButtonVisibility = (PlugDeviceListSources.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

            TimeSpan period = TimeSpan.FromSeconds(2);
            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (PlugDeviceListSources.Count == 0 ||
                        PlugDeviceListSources.Count < ZigbeeDeviceService.ZigbeeDeviceListSources.Count)
                    {
                        PlugDeviceListSources = ZigbeeDeviceService.ZigbeeDeviceListSources;
                        SaveButtonVisibility = (PlugDeviceListSources.Count > 0) ? Visibility.Visible : Visibility.Collapsed;
                    }
                });
            }, period);

            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);

            PlugSelectionChangedCommand = new RelayCommand<ZigBeeEndDevice>(PlugSelectionChanged);

            SaveCommand = new RelayCommand(Save);
        }

        private void BackButtonClicked()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private void PlugSelectionChanged(ZigBeeEndDevice plugDevice)
        {
            PlugDeviceSelectedItem = plugDevice;
        }

        private void Save()
        {
            Debug.WriteLine("Save!!");
            ZigbeeDeviceService.SetEndDevices(PlugDeviceListSources);
        }
    }
}
