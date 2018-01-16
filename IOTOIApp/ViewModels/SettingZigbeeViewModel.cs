using IOTOIApp.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Newtonsoft.Json;
using IOTOI.Model.ZigBee;

namespace IOTOIApp.ViewModels
{
    public class SettingZigbeeViewModel : ViewModelBase
    {
        private ObservableCollection<ZigBeeEndDevice> _endDeviceListSources;
        public ObservableCollection<ZigBeeEndDevice> EndDeviceListSources
        {
            get { return _endDeviceListSources; }
            set { Set(ref _endDeviceListSources, value); }
        }

        private object _endDeviceSelected;
        public object EndDeviceSelected
        {
            get { return _endDeviceSelected; }
            set { Set(ref _endDeviceSelected, value); }
        }

        private Visibility _diceDiscoveryVisibility = Visibility.Collapsed;
        public Visibility DeviceDiscoveryVisibility
        {
            get { return _diceDiscoveryVisibility; }
            set { Set(ref _diceDiscoveryVisibility, value); }
        }

        public ListView EndDeviceListView;

        public ICommand CloseCommand { get; private set; }
        public ICommand LampToggleCommand { get; private set; }
        public ICommand PlugPowerOnCommand { get; private set; }
        public ICommand PlugPowerOffCommand { get; private set; }

        public ICommand InitialEndDeviceListCommand { get; private set; }
        public ICommand DeviceDiscoveryCommand { get; private set; }

        public ICommand DisconnectCommand { get; private set; }

        public SettingZigbeeViewModel()
        {
            //CloseCommand = new RelayCommand<EndDevice>(PowerOff);
            LampToggleCommand = new RelayCommand<ZigBeeEndPoint>(LampToggle);

            PlugPowerOnCommand = new RelayCommand<ZigBeeEndPoint>(PlugPowerOn);
            PlugPowerOffCommand = new RelayCommand<ZigBeeEndPoint>(PlugPowerOff);

            InitialEndDeviceListCommand = new RelayCommand(InitialEndDeviceList);
            DeviceDiscoveryCommand = new RelayCommand(DeviceDiscovery);

            DisconnectCommand = new RelayCommand<ZigBeeEndDevice>(Disconnect);

            //EndDeviceListSources = new ObservableCollection<EndDevice>(XBeeAction.DiscoverEndDevices());

            SetupEndDevice();
        }
        

        //private AppServiceConnection connection;
        //private static AppServiceConnection connection = IOTOI.Shared.Model.Connection.connection;

        //private async Task<bool> GetConnection()
        //{
        //    if (connection == null)
        //    {
        //        connection = new AppServiceConnection();
        //        connection.AppServiceName = "com.commax.common";
        //        connection.PackageFamilyName = "IOTOI.Provider_fxs4yby6q8m3m";

        //        var status = await connection.OpenAsync();


        //        switch (status)
        //        {
        //            case AppServiceConnectionStatus.AppNotInstalled:
        //                Debug.WriteLine("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.");
        //                return false;

        //            case AppServiceConnectionStatus.AppUnavailable:
        //                Debug.WriteLine("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.");
        //                return false;

        //            case AppServiceConnectionStatus.AppServiceUnavailable:
        //                Debug.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", connection.AppServiceName));
        //                return false;

        //            case AppServiceConnectionStatus.Unknown:
        //                Debug.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
        //                return false;
        //        }

        //    }


        //    return true;

        //}

        public void SetupEndDevice()
        {   
            TimeSpan period = TimeSpan.FromSeconds(3);
            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                //InitialEndDeviceList();

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    InitialEndDeviceList();

                    var message = new ValueSet();
                    message.Add("Type", "ZigBee");
                    message.Add("Command", "GetDiscoverRunning");

                    var rtnMessage = new ValueSet();
                    rtnMessage = IOTOI.Common.CommonService.GetReturnData(message);
                    bool ird = Convert.ToBoolean(rtnMessage["Result"]);
                    DeviceDiscoveryVisibility = ird ? Visibility.Collapsed : Visibility.Visible;
                });

            }, period);
        }
        
        
        public async void InitialEndDeviceList()
        {   
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    var message = new ValueSet();
                    message.Add("Type", "ZigBee");
                    message.Add("Command", "GetEndDevices");
                    
                    EndDeviceListSources = JsonConvert.DeserializeObject<ObservableCollection<ZigBeeEndDevice>>(IOTOI.Common.CommonService.GetReturnData(message)["Result"].ToString());

                }
                catch (Exception e)
                {
                    String test = e.Message;
                }
            });
        }

        private void DeviceDiscovery()
        {
            var message = new ValueSet();
            message.Add("Command", "Discover");
            message.Add("Type", "ZigBee");
            
            IOTOI.Common.CommonService.GetReturnData(message);

        }


        private void PlugPowerOn(ZigBeeEndPoint endPoint)
        {
            Debug.WriteLine("Call PlugPowerOn !! " + endPoint.MacAddress + " # " + endPoint.Id);

            if (endPoint == null || false == endPoint.IsActivated) return;

            try
            {   
                var message = new ValueSet();

                message.Add("Type", "ZigBee");
                message.Add("Command", "PlugPowerOn");
                message.Add("endPoint", JsonConvert.SerializeObject(endPoint));
                
                IOTOI.Common.CommonService.GetReturnData(message);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message, "ZigBee Plug Power On Exception : ");
            }
        }

        private void PlugPowerOff(ZigBeeEndPoint endPoint)
        {
            Debug.WriteLine("Call PlugPowerOff !! " + endPoint.MacAddress + " # " + endPoint.Id);

            if (endPoint == null || false == endPoint.IsActivated) return;

            try
            {   
                var message = new ValueSet();

                message.Add("Type", "ZigBee");
                message.Add("Command", "PlugPowerOff");
                message.Add("endPoint", JsonConvert.SerializeObject(endPoint));
                
                IOTOI.Common.CommonService.GetReturnData(message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message, "ZigBee Plug Power Off Exception : ");
            }
        }

        private void LampToggle(ZigBeeEndPoint endPoint)
        {
            Debug.WriteLine("Call LampToggle !! " + endPoint.MacAddress + " # " + endPoint.Id);
            if (endPoint == null) return;

            try
            {   
                var message = new ValueSet();

                message.Add("Type", "ZigBee");
                message.Add("Command", "OnOffToggle");
                message.Add("endPoint", JsonConvert.SerializeObject(endPoint));
                
                IOTOI.Common.CommonService.GetReturnData(message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message, "LampToggle Exception : ");
            }

        }

        private void Disconnect(ZigBeeEndDevice endDevice)
        {
            Debug.WriteLine("Call Disconnect !! ");
            if (endDevice == null) return;

            try
            {   
                var message = new ValueSet();

                message.Add("Type", "ZigBee");
                message.Add("Command", "ManagementLeave");
                message.Add("endDevice", JsonConvert.SerializeObject(endDevice));
                
                IOTOI.Common.CommonService.GetReturnData(message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message, "Management Leave Exception : ");
            }
        }
    }
}

