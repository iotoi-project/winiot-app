using MetroLog;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace IOTOI.Common
{
    public static class CommonService
    {
        static Model.Db.Context db;
        static IOTOI.Model.LoggingServices.LoggingServices loggingServices = new Model.LoggingServices.LoggingServices();
        static ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(CommonService));


        private const string TypeZigBee = "ZIGBEE";

        private static string[] Types = new string[] { TypeZigBee };

        static string strNestToken = "";

        #region GetReturnData
        public static ValueSet GetReturnData(ValueSet message)
        {
            ValueSet returnData = new ValueSet();

            string type = message["Type"] as string;

            switch (type.ToUpper())
            {
                case "COMMON":
                    GetReturnData(message, ref returnData);
                    break;
                case "ZIGBEE":
                    ZigBeeService.GetReturnData(message, ref returnData);
                    //zigBeeService.GetReturnData(message, ref returnData);
                    break;
                default:
                    {
                        returnData.Add("Status", "Fail: unknown Type");
                        break;
                    }
            }

            return returnData;

        }

        private static void GetReturnData(ValueSet message, ref ValueSet returnData)
        {
            string command = message["Command"] as string;
            switch (command.ToUpper())
            {
                case "INIT":
                    {
                        Hashtable result = CommonInit();
                        string status = "OK";
                        if (!(bool)result[TypeZigBee]) status = "\nFail to init ZigBee";
                        returnData.Add("Status", status);
                    }
                    break;
                case "GETPHRASELIST":
                    {
                        List<string> result = null;
                        try
                        {
                            using (var db = new IOTOI.Model.Db.Context())
                            {
                                result = (from ep in db.ZigBeeEndPoint
                                          where ep.CustomName != null
                                          select ep.CustomName)
                                          .ToList();
                            }
                        }
                        catch (Exception e)
                        {
                            loggingServices.Write(Log, e.Message, LogLevel.Error, e);
                        }
                        returnData.Add("Result", JsonConvert.SerializeObject(result));
                        returnData.Add("Status", "OK");
                    }
                    break;
                case "GETDEVICE":
                    {
                        returnData.Add("Result", JsonConvert.SerializeObject(ZigBeeService.GetEndPoint(message["Target"] as string)));
                        returnData.Add("Status", "OK");
                    }
                    break;
                case "GETSTATUS":
                    {
                        returnData.Add("Result", ZigBeeService.GetEndPointStatus(message["Target"] as string));
                        returnData.Add("Status", "OK");
                    }
                    break;
                case "SETNESTTOKEN":
                    {
                        strNestToken = message["NESTTOKEN"].ToString();

                        Nest.NestCommandHandler.ThermostatAPI.ApplyAccessToken(strNestToken);

                        returnData.Add("Status", "OK");
                    }
                    break;
                case "GETNESTSTATUS":
                    {
                        //TODO : NEST GET STATUS
                        //Result CASE 1(temperature : 60)
                        //Result CASE 2(fan : true or false)

                        #region 
                        if (message["Target"].ToString() == "temperature")
                        {
                            string r = Nest.NestCommandHandler.CurrentTemperature();
                            if (r.Contains("Fail:"))
                            {
                                returnData.Add("Status", r);
                            }
                            else
                            {
                                returnData.Add("Result", r);
                                returnData.Add("Status", "OK");
                            }
                        }
                        else
                        if (message["Target"].ToString() == "fan")
                        {
                            string r = Nest.NestCommandHandler.CurrentFanStatus();
                            if (r.Contains("Fail:"))
                            {
                                returnData.Add("Status", r);
                            }
                            else
                            {
                                returnData.Add("Result", r);    // FanRunning , FanStopped
                                returnData.Add("Status", "OK");
                            }
                        }
                        #endregion
                    }
                    break;
                default:
                    {
                        returnData.Add("Status", "Fail: unknown command");
                        break;
                    }
            }
        }
        #endregion

        #region CommonInit
        private static Hashtable CommonInit()
        {
            Hashtable result = new Hashtable();

#if DEBUG
            //ClearDB();
#endif

            MigrateDB();

            result.Add(TypeZigBee, true);
            if (StartZigBeeInit() != 0)
            {
                result[TypeZigBee] = false;
                loggingServices.Write(Log, "Fail to init ZigBee");
            }

            return result;

        }
        #endregion

        #region ClearDB
        [Conditional("DEBUG")]
        private static void ClearDB()
        {

            //Debug.WriteLine("clearDB");                        
            loggingServices.Write(Log, "clearDB", LogLevel.Debug);
            if (System.IO.File.Exists(Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\iotoiapp.db"))
            {
                // Use a try block to catch IOExceptions, to
                // handle the case of the file already being
                // opened by another process.
                try
                {
                    System.IO.File.Delete(Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\iotoiapp.db");
                }
                catch (System.IO.IOException e)
                {
                    loggingServices.Write(Log, e.Message, LogLevel.Error, e);
                    return;
                }
            }

        }
        #endregion

        #region MigrateDB
        private static void MigrateDB()
        {
            //Debug.WriteLine("MigrateDB");
            //loggingServices.WriteLine<CommonService>("MigrateDB", LogLevel.Debug);
            loggingServices.Write(Log, "MigrateDB", LogLevel.Debug);
            db = new Model.Db.Context();
            db.Database.Migrate();

            #region INSERT OR IGNORE
            foreach (string strType in Types)
            {
                var Ptype = db.ProtocolType.Where(u => u.Name == strType).FirstOrDefault();
                if (Ptype == null)
                {
                    db.ProtocolType.Add(new Model.Common.ProtocolType { Name = strType });
                }
            }
            #endregion



            #region TEstData            
            //List<CommaxIot.Model.ZigBee.ZigBeeEndPoint> endPoints = new List<CommaxIot.Model.ZigBee.ZigBeeEndPoint>();
            //ushort mc = Convert.ToUInt16(6066005671910370);
            //db.Space.Add(new Model.Common.Space { Name = "Living Room" });
            //endPoints.Add(new CommaxIot.Model.ZigBee.ZigBeeEndPoint
            //{
            //    CommanProfileId = 260,
            //    DeviceId = 81,
            //    EpNum = 1,
            //    Name = "Unknown device type",
            //    CustomName = "Lamp",
            //    MacAddress = mc,
            //    ProtocolTypeId = 1,
            //    SpaceId = 1,
            //    ZigBeeInClusters = new List<Model.ZigBee.ZigBeeInCluster>() { new Model.ZigBee.ZigBeeInCluster { ClusterId = "0006" } },
            //    ZigBeeOutClusters = new List<Model.ZigBee.ZigBeeOutCluster>() { new Model.ZigBee.ZigBeeOutCluster { ClusterId = "0500" } },
            //    IsOpen = null
            //});

            ////Model.ZigBee.ZigBeeInCluster zc = new Model.ZigBee.ZigBeeInCluster { ClusterId = "0006" };

            //var enddevice = new CommaxIot.Model.ZigBee.ZigBeeEndDevice
            //{
            //    MacAddress = mc,
            //    NetworkAddress = 7112,
            //    EndPoints = endPoints
            //};

            //db.ZigBeeEndDevice.Add(enddevice);
            #endregion

            db.SaveChanges();
        }
        #endregion

        private static int StartZigBeeInit()
        {
            //zigBeeService = new ZigBeeService();
            //return zigBeeService.InitZigBee();
            return ZigBeeService.InitZigBee();
        }
    }
}
