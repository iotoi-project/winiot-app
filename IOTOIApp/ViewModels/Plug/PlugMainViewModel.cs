using System;

using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Popups;
using IOTOIApp.Services;
using IOTOI.Model.ZigBee;
using Microsoft.Practices.ServiceLocation;
using Windows.System.Threading;
using Windows.UI.Core;
using Newtonsoft.Json;
using Windows.Foundation.Collections;
using System.Linq;
using Windows.UI.Xaml;

namespace IOTOIApp.ViewModels.Plug
{
    public class PlugMainViewModel : ViewModelBase
    {
        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        private ObservableCollection<ZigBeeEndDevice> _plugDeviceListSources = new ObservableCollection<ZigBeeEndDevice>();
        public ObservableCollection<ZigBeeEndDevice> PlugDeviceListSources
        {
            get { return _plugDeviceListSources; }
            set { Set(ref _plugDeviceListSources, value); }
        }


        private ZigBeeEndDevice _plugDeviceSelectedItem;
        public ZigBeeEndDevice PlugDeviceSelectedItem
        {
            get { return _plugDeviceSelectedItem;  }
            set { Set(ref _plugDeviceSelectedItem, value); }
        }

        private Visibility _notFoundMsgVisibility = Visibility.Collapsed;
        public Visibility NotFoundMsgVisibility
        {
            get { return _notFoundMsgVisibility; }
            set { Set(ref _notFoundMsgVisibility, value); }
        }

        private Visibility _settingsButtonVisibility = Visibility.Collapsed;
        public Visibility SettingsButtonVisibility
        {
            get { return _settingsButtonVisibility; }
            set { Set(ref _settingsButtonVisibility, value); }
        }

        public ICommand BackButtonClickedCommand { get; private set; }

        public ICommand GoSettingsPageCommand { get; private set; }

        public ICommand PowerOnCommand { get; private set; }
        public ICommand PowerOffCommand { get; private set; }

        public ICommand AllOffButtonClickCommand { get; private set; }

        public ICommand PlugSelectionChangedCommand { get; private set; }

        ThreadPoolTimer PeriodicTimer;

        public PlugMainViewModel()
        {
            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);

            GoSettingsPageCommand = new RelayCommand(GoSettingsPage);

            PowerOnCommand = new RelayCommand<ZigBeeEndPoint>(PowerOn);
            PowerOffCommand = new RelayCommand<ZigBeeEndPoint>(PowerOff);

            AllOffButtonClickCommand = new RelayCommand<object>(AllOffButtonClick);
        }

        public void InitDeviceStatusTH()
        {
            CloseTH();

            ZigbeeDeviceService.GetEndDevices(81);
            StartTH();
        }

        public void StartTH()
        {
            Debug.WriteLine("StartTH()");
            
            bool RunTimmer = true;

            TimeSpan period = TimeSpan.FromMilliseconds(200);
            PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                //if (ConnectionService.isBackground) return;
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    try
                    {
                        if (RunTimmer)
                        {
                            RunTimmer = false;

                            if (PlugDeviceListSources.Count == 0 || (PlugDeviceListSources.Count != ZigbeeDeviceService.ZigbeeDeviceCount))
                            {
                                if (ZigbeeDeviceService.ZigbeeDeviceCount != -1)
                                {
                                    PlugDeviceListSources = ZigbeeDeviceService.ZigbeeDeviceListSources;

                                    if (PlugDeviceListSources.Count > 0)
                                    {
                                        NotFoundMsgVisibility = Visibility.Collapsed;
                                        SettingsButtonVisibility = Visibility.Visible;
                                    }
                                    else
                                    {
                                        NotFoundMsgVisibility = Visibility.Visible;
                                        SettingsButtonVisibility = Visibility.Collapsed;
                                    }
                                }
                            }
                            else
                            {
                                if (ZigbeeDeviceService.ZigbeeDeviceCount > 0)
                                {
                                    for (int i = 0; i < PlugDeviceListSources.Count; i++)
                                    {
                                        if (JsonConvert.SerializeObject(PlugDeviceListSources[i]) != JsonConvert.SerializeObject(ZigbeeDeviceService.ZigbeeDeviceListSources[i]))
                                        {
                                            PlugDeviceListSources[i] = ZigbeeDeviceService.ZigbeeDeviceListSources[i];
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("STartTH Exception : " + e.Message);
                    }
                    finally
                    {
                        RunTimmer = true;
                    }

                });
            }, period);
        }

        public void CloseTH()
        {
            Debug.WriteLine("CloseTH");
            if (PeriodicTimer != null) PeriodicTimer.Cancel();
            ZigbeeDeviceService.CloseEndDevices();
        }

        private void GoSettingsPage()
        {
            NavigationService.Navigate("IOTOIApp.ViewModels.Plug.PlugSettingViewModel");
        }

        private void PowerOn(ZigBeeEndPoint endPoint)
        {
            Debug.WriteLine("Call PowerOn !! " + endPoint.MacAddress + " # " + endPoint.Id);

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

        private void PowerOff(ZigBeeEndPoint endPoint)
        {
            Debug.WriteLine("Call PowerOff !! " + endPoint.MacAddress + " # " + endPoint.Id);

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

        private void AllOffButtonClick(object obj)
        {
            if (obj is ZigBeeEndDevice)
            {
                AllOff(obj as ZigBeeEndDevice);
            }
            else
            {
                foreach (ZigBeeEndDevice endDevice in PlugDeviceListSources)
                {
                    AllOff(endDevice);
                }
            }
        }

        private void AllOff(ZigBeeEndDevice endDevice)
        {
            Debug.WriteLine("Call AllOff !! " + endDevice.MacAddress + " # " + endDevice.EndPoints.Count);

            foreach (ZigBeeEndPoint endPoint in endDevice.EndPoints)
            {
                foreach (ZigBeeInCluster ZigBeeInCluster in endPoint.ZigBeeInClusters.Where(z => z.ClusterId == 6))
                {
                    foreach (ZigBeeInClusterAttribute zigBeeInClusterAttribute in ZigBeeInCluster.ZigBeeInClusterAttributes)
                    {
                        if ((bool)zigBeeInClusterAttribute.RealValue)
                        {
                            PowerOff(endPoint);
                        }
                    }
                }
            }
        }

        public void PlugSelectionChanged(ZigBeeEndDevice plugDevice)
        {
            try
            {
                PlugDeviceSelectedItem = plugDevice;
            }
            catch (Exception e)
            {
                Debug.WriteLine("PlugSelectionChanged Exception : " + e.Message);
            }

        }

        private void BackButtonClicked()
        {
            if (NavigationService.CanGoBack)
            {
                var ShellVM = ServiceLocator.Current.GetInstance<ShellViewModel>();
                ShellVM.NaviToSettingPage(false);
                NavigationService.GoBack();

                CloseTH();
            }
        }
    }
}
