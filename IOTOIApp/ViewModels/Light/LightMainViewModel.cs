using System;

using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Newtonsoft.Json;
using Windows.System.Threading;
using Windows.Foundation;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.Practices.ServiceLocation;
using IOTOIApp.Services;
using IOTOI.Model.ZigBee;
using IOTOIApp.Models;
using IOTOIApp.ViewModels;
using Windows.UI.Xaml;

namespace IOTOIApp.ViewModels.Light
{
    public class LightMainViewModel : ViewModelBase
    {
        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        private ObservableCollection<ZigBeeEndDevice> _lightDeviceListSources = new ObservableCollection<ZigBeeEndDevice>();
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
        public ICommand TurnOnLightCommand { get; private set; }
        public ICommand TurnOffLightCommand { get; private set; }

        public ICommand ToggleOnOffCommand { get; private set; }
        public ICommand AllOffButtonClickCommand { get; private set; }

        public ICommand DimDownCommand { get; private set; }
        public ICommand DimUpCommand { get; private set; }

        public ICommand LightSelectionChangedCommand { get; private set; }

        ThreadPoolTimer PeriodicTimer;

        public LightMainViewModel()
        {
            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);

            GoSettingsPageCommand = new RelayCommand(GoSettingsPage);

            ToggleOnOffCommand = new RelayCommand<ZigBeeEndPoint>(ToggleOnOff);

            AllOffButtonClickCommand = new RelayCommand<object>(AllOffButtonClick);

            DimDownCommand = new RelayCommand<ZigBeeEndPoint>(DimDown);
            DimUpCommand = new RelayCommand<ZigBeeEndPoint>(DimUp);

            LightSelectionChangedCommand = new RelayCommand<ZigBeeEndDevice>(LightSelectionChanged);
        }

        public void InitDeviceStatusTH()
        {
            CloseTH();

            ZigbeeDeviceService.GetEndDevices(256);
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

                            if (LightDeviceListSources.Count == 0 || LightDeviceListSources.Count != ZigbeeDeviceService.ZigbeeDeviceCount)
                            {
                                if(ZigbeeDeviceService.ZigbeeDeviceCount != -1)
                                {
                                    LightDeviceListSources = ZigbeeDeviceService.ZigbeeDeviceListSources;

                                    if (LightDeviceListSources.Count > 0)
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
                                if(ZigbeeDeviceService.ZigbeeDeviceCount > 0)
                                {
                                    for (int i = 0; i < LightDeviceListSources.Count; i++)
                                    {
                                        if (JsonConvert.SerializeObject(LightDeviceListSources[i]) != JsonConvert.SerializeObject(ZigbeeDeviceService.ZigbeeDeviceListSources[i]))
                                        {
                                            LightDeviceListSources[i] = ZigbeeDeviceService.ZigbeeDeviceListSources[i];
                                        }
                                    }
                                }
                            }
                        }

                    }catch(Exception e)
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
            NavigationService.Navigate("IOTOIApp.ViewModels.Light.LightSettingViewModel");
        }

        private void ToggleOnOff(ZigBeeEndPoint endPoint)
        {
            Debug.WriteLine("Call ToggleOnOff !! " + endPoint.MacAddress + " # " + endPoint.Id);
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

        private void AllOffButtonClick(object obj)
        {
            if (obj is ZigBeeEndDevice)
            {
                AllOff(obj as ZigBeeEndDevice);
            }
            else
            {
                foreach(ZigBeeEndDevice endDevice in LightDeviceListSources)
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
                            ToggleOnOff(endPoint);
                        }
                    }
                }
            }
        }

        private void DimDown(ZigBeeEndPoint endPoint)
        {
            Debug.WriteLine("DimDown !!!");
            foreach (var EndDevice in LightDeviceListSources)
            {
                foreach (var l in EndDevice.EndPoints.Where(w => w.Id == endPoint.Id))
                {
                    LightDeviceSelectedItem = EndDevice;
                }
            }
        }

        private void DimUp(ZigBeeEndPoint endPoint)
        {
            Debug.WriteLine("DimUp !!!");
            foreach (var EndDevice in LightDeviceListSources)
            {
                foreach (var l in EndDevice.EndPoints.Where(w => w.Id == endPoint.Id))
                {
                    LightDeviceSelectedItem = EndDevice;
                }
            }
        }

        public void LightSelectionChanged(ZigBeeEndDevice lightDevice)
        {
            try
            {
                LightDeviceSelectedItem = lightDevice;
            }catch(Exception e)
            {
                Debug.WriteLine("LightSelectionChanged Exception : "+ e.Message);
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
