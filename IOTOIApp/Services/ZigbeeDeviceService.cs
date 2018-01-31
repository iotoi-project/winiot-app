using IOTOIApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using IOTOI.Model.ZigBee;
using IOTOIApp.Utils;

namespace IOTOIApp.Services
{
    public class ZigbeeDeviceService
    {
        public static ObservableCollection<ZigBeeEndDevice> ZigbeeDeviceListSources = new ObservableCollection<ZigBeeEndDevice>();
        public static ObservableCollection<ZigBeeEndDevice> TmpZigbeeDeviceListSources = new ObservableCollection<ZigBeeEndDevice>();

        private static ThreadPoolTimer PeriodicTimer;        

        public static void CloseEndDevices()
        {
            Debug.WriteLine("CloseEndDevices");
            if(PeriodicTimer != null) PeriodicTimer.Cancel();
            ZigbeeDeviceListSources.Clear();
        }

        public static void GetEndDevices(ushort deviceId = 0)
        {
            bool RunTimmer = true;

            TimeSpan period = TimeSpan.FromMilliseconds(1000);
            PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        Debug.WriteLine("GetEndDevices RunTimmer : " + RunTimmer);
                        if (RunTimmer)
                        {
                            RunTimmer = false;

                            var message = new ValueSet();
                            message.Add("Type", "ZigBee");
                            message.Add("Command", "GetEndDevices");

                            ZigbeeDeviceListSources = JsonConvert.DeserializeObject<ObservableCollection<ZigBeeEndDevice>>(IOTOI.Common.CommonService.GetReturnData(message)["Result"].ToString());

                            bool IsDevice = false;

                            for(int i = ZigbeeDeviceListSources.Count - 1; i >=0; i--)
                            { 
                            //foreach (ZigBeeEndDevice endDevice in ZigbeeDeviceListSources)
                            //{
                                foreach (ZigBeeEndPoint endPoint in ZigbeeDeviceListSources[i].EndPoints)
                                {
                                    if (endPoint.DeviceId == deviceId)
                                    {
                                        IsDevice = true;
                                        endPoint.CustomName = String.IsNullOrEmpty(endPoint.CustomName) ? "Device " + endPoint.EpNum : endPoint.CustomName;
                                    }
                                    else
                                    {
                                        IsDevice = false;
                                        break;
                                    }
                                }

                                if (!IsDevice) ZigbeeDeviceListSources.RemoveAt(i);
                            }

                            RunTimmer = true;
                        }
                        
                    }
                    catch (Exception e)
                    {
                        String test = e.Message;
                        RunTimmer = true;
                        Debug.WriteLine("GetEndDevices Exception : " + e.Message);
                    }
                });

            }, period);
        }

        public static async void SetEndDevices(ObservableCollection<ZigBeeEndDevice> collection)
        {
            List<ValueSet> list = new List<ValueSet>();
            foreach (ZigBeeEndDevice endDevice in collection)
            {
                foreach (ZigBeeEndPoint endPoint in endDevice.EndPoints)
                {
                    ValueSet vs = new ValueSet();
                    Debug.WriteLine(string.Format("endPoint.Id {0} #  endPoint.CustomName {1}", endPoint.Id, endPoint.CustomName));
                    list.Add(new ValueSet { { "Id", endPoint.Id },{ "Name", endPoint.CustomName } });
                    
                }
            }

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
            try
            {
                var message = new ValueSet();
                message.Add("Type", "ZigBee");
                message.Add("Command", "SetEndDevices");
                message.Add("Param", JsonConvert.SerializeObject(list));

                ValueSet ReturnData = IOTOI.Common.CommonService.GetReturnData(message);

                Debug.WriteLine(ReturnData["Status"].ToString());

                string phList = null;
                if (ReturnData.ContainsKey("Status"))
                {
                    if (ReturnData["Status"] != null)
                    {
                        Debug.WriteLine(ReturnData["Status"].ToString());
                        phList = ReturnData["Result"].ToString();
                    }
                }

                #region Call Cortana Update
                VoiceCommandHandler voiceCommandHandler = new VoiceCommandHandler();
                await Task.Run(async () => await voiceCommandHandler.SetPhraseList(phList));
                    #endregion
                }
                catch (Exception e)
                {
                    String test = e.Message;
                }
            });
        }

    }
}
