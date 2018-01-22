using System;

using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using IOTOIApp.Services;

namespace IOTOIApp.ViewModels.Sensor
{
    public class SensorDetailViewModel : ViewModelBase
    {
        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        string _sensorType;
        public string SensorType
        {
            get { return _sensorType; }
            set
            {
               Set(ref _sensorType, value);

                SensorDeviceListSources = SensorDeviceService.GetSensorList(value);
            }
        }

        ObservableCollection<Models.Sensor> _sensorDeviceListSources = null;
        public ObservableCollection<Models.Sensor> SensorDeviceListSources
        {
            get { return _sensorDeviceListSources; }
            set { Set(ref _sensorDeviceListSources, value); }
        }

        public ICommand BackButtonClickedCommand { get; private set; }

        public SensorDetailViewModel()
        {
            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);
        }

        void BackButtonClicked()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
