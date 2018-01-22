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

namespace IOTOIApp.Services
{
    public class SensorDeviceService
    {
        static AppServiceConnection connection;
        static AppServiceConnection connectionMainApp;


        public static ObservableCollection<Sensor> GetSensorList(string sensorType)
        {
            /*
            TimeSpan period = TimeSpan.FromSeconds(2);
            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        //if (!await GetConnection()) return;
                        if (!await IOTOI.Shared.Model.Connection.GetConnection()) return;
                        var message = new ValueSet();
                        message.Add("Type", "ZigBee");
                        message.Add("Command", "GetEndDevices");

                        connection = IOTOI.Shared.Model.Connection.connection;

                        AppServiceResponse response = await connection.SendMessageAsync(message);

                        if (response.Status == AppServiceResponseStatus.Success && response.Message.ContainsKey("Result"))
                        {
                            if (response.Message["Result"] != null)
                            {
                                Debug.WriteLine(response.Message["Result"].ToString());
                                LightDeviceListSources = JsonConvert.DeserializeObject<ObservableCollection<Models.EndDevice>>(response.Message["Result"].ToString());

                            }
                        }
                    }
                    catch (Exception e)
                    {
                        String test = e.Message;
                    }
                });

            }, period);
            */

            ObservableCollection<Sensor> list = new ObservableCollection<Sensor>();

            for (int i = 0; i < 10; ++i)
            {
                Sensor s = new Sensor()
                {
                    SensorType = ("Motion Sensor" == sensorType) ? 0 : 1,
                    SensorId = i + 1,
                    SensorNo = (short)(i + 1),
                    SensorName = string.Format("{0:00}", i + 1) + " - " + sensorType,
                    SensorViewTitle = "Room " + (i + 1).ToString(),
                    SensorStatus = (0 == i) ? 0 : i % 3
                };

                list.Add(s);
            }

            return list;
        }


        public static async void SetSensors(string sensorType, ObservableCollection<Sensor> collection)
        {
            /*
            List<ValueSet> list = new List<ValueSet>();
            foreach (EndDevice endDevice in collection)
            {
                foreach (EndPoint endPoint in endDevice.EndPoints)
                {
                    ValueSet vs = new ValueSet();
                    Debug.WriteLine(string.Format("endPoint.Id {0} #  endPoint.CustomName {1}", endPoint.Id, endPoint.CustomName));
                    list.Add(new ValueSet { { "Id", endPoint.Id }, { "Name", endPoint.CustomName } });

                }
            }
            */
            
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                /*
                try
                {
                    if (!await IOTOI.Shared.Model.Connection.GetConnection()) return;
                    var message = new ValueSet();
                    message.Add("Type", "ZigBee");
                    message.Add("Command", "SetEndDevices");
                    message.Add("Param", JsonConvert.SerializeObject(list));

                    connection = IOTOI.Shared.Model.Connection.connection;

                    AppServiceResponse response = await connection.SendMessageAsync(message);

                    string phList = null;
                    if (response.Status == AppServiceResponseStatus.Success && response.Message.ContainsKey("Status"))
                    {
                        if (response.Message["Status"] != null)
                        {
                            Debug.WriteLine(response.Message["Status"].ToString());
                            phList = response.Message["Result"].ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    String test = e.Message;
                }
                */
            });
            
        }
    }
}
