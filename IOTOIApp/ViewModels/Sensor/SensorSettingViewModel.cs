using System;

using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Diagnostics;
using IOTOIApp.Services;

namespace IOTOIApp.ViewModels.Sensor
{
    public class SensorSettingViewModel : ViewModelBase
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

                GetSensorList(value);
            }
        }

        ObservableCollection<Models.Sensor> _sensorDeviceListSources = null;
        public ObservableCollection<Models.Sensor> SensorDeviceListSources
        {
            get { return _sensorDeviceListSources; }
            set { Set(ref _sensorDeviceListSources, value); }
        }

        public ICommand BackButtonClickedCommand { get; private set; }
        public ICommand SettingCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public SensorSettingViewModel()
        {
            SensorDeviceListSources = new ObservableCollection<Models.Sensor>();

            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);
            SettingCommand = new RelayCommand<string>(SettingChecked);
            SaveCommand = new RelayCommand(Save);
        }

        void BackButtonClicked()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        void SettingChecked(string sensorType)
        {
            SensorType = sensorType;
        }

        void GetSensorList(string sensorType)
        {
            ObservableCollection<Models.Sensor> list = SensorDeviceService.GetSensorList(sensorType);

            SensorDeviceListSources.Clear();

            foreach (Models.Sensor itm in list)
                SensorDeviceListSources.Add(itm);

            list.Clear();
            list = null;
        }

        void Save()
        {
            Debug.WriteLine("Save!!");

            //foreach (EndDevice endDevice in LightDeviceListSources)
            //{
            //    foreach (EndPoint endPoint in endDevice.EndPoints) { 
            //        Debug.WriteLine(string.Format("endPoint.Id {0} #  endPoint.CustomName {1}", endPoint.Id, endPoint.CustomName));
            //    }
            //}

            SensorDeviceService.SetSensors(SensorType, SensorDeviceListSources);
        }
    }
}
