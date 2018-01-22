using MetroLog;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace IOTOI.Common
{
    public static class ZigBeeService
    {
        static ZigbeeAdapterLib.Adapter ZigBeeAdapter = null;

        private static int IsZigBeeStatus = -1;

        static IOTOI.Model.LoggingServices.LoggingServices loggingServices = new Model.LoggingServices.LoggingServices();
        static ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(ZigBeeService));


        #region Init ZIgBee
        public static int InitZigBee()
        {
            if (ZigBeeAdapter == null)
            {
                InitTable();
                ZigBeeAdapter = new ZigbeeAdapterLib.Adapter();
                IsZigBeeStatus = (int)ZigBeeAdapter.Initialize();
                loggingServices.Write(Log, "initialize IsZigBeeStatus :: " + IsZigBeeStatus, LogLevel.Debug);
            }
            else if (IsZigBeeStatus != 0)
            {
                InitTable();
                IsZigBeeStatus = (int)ZigBeeAdapter.Initialize();
                loggingServices.Write(Log, "initialize IsZigBeeStatus :: " + IsZigBeeStatus, LogLevel.Debug);
            }

            return IsZigBeeStatus;

        }
        #endregion

        #region StartDeviceDiscovery()
        private static void StartDeviceDiscovery()
        {
            InitTable();
            ZigBeeAdapter.StartDeviceDiscovery();
        }
        #endregion

        #region IsRunningDiscoverDevices
        private static bool IsRunningDiscoverDevices()
        {
            return ZigBeeAdapter.GetRunningDiscoverDevices();
        }
        #endregion

        #region GetEndDevices()
        private static List<IOTOI.Model.ZigBee.ZigBeeEndDevice> GetEndDevices()
        {
            List<IOTOI.Model.ZigBee.ZigBeeEndDevice> result = null;
            using (var db = new IOTOI.Model.Db.Context())
            {
                try
                {

                    result = db.ZigBeeEndDevice
                        .Include(b => b.EndPoints)
                            .ThenInclude(ba => ba.ZigBeeInClusters)
                            .ThenInclude(bc => bc.ZigBeeInClusterAttributes)
                        .Include(b => b.EndPoints)
                            .ThenInclude(ba => ba.ZigBeeOutClusters)
                            .ThenInclude(bc => bc.ZigBeeOutClusterAttributes)
                        .ToList();

                }
                catch (Exception e)
                {
                    loggingServices.Write(Log, e.Message, LogLevel.Error, e);
                }

                return result;
            }
        }
        #endregion

        #region SetEndDevices()
        private static List<string> SetEndDevices(List<ValueSet> list)
        {
            List<string> rtn = null;

            using (var db = new IOTOI.Model.Db.Context())
            {
                foreach (ValueSet vs in list)
                {
                    var result = db.ZigBeeEndPoint.SingleOrDefault(e => e.Id == (int)vs["Id"]);
                    if (result != null)
                    {
                        result.CustomName = (string)vs["Name"];
                    }
                }
                db.SaveChanges();

                rtn = (from ep in db.ZigBeeEndPoint
                       where ep.CustomName != null
                       select ep.CustomName)
                                  .ToList();
            }
            return rtn;

        }
        #endregion

        #region ELZ FIND ENDPOINT TEST
        //추후 코타나에서 사용할때 이용할 예정
        public static Model.ZigBee.ZigBeeEndPoint GetEndPoint(string target)
        {
            //int id = 0;
            Model.ZigBee.ZigBeeEndPoint endPoint = null;
            using (var db = new IOTOI.Model.Db.Context())
            {
                try
                {
                    //id = (from ep in db.ZigBeeEndPoint
                    //      where (ep.CustomName).ToUpper().Replace(" ", "") == target.ToUpper().Replace(" ", "")
                    //      select ep.Id)
                    //     .FirstOrDefault();
                    endPoint = db.ZigBeeEndPoint
                        .Where(
                            b => b.CustomName != null && b.CustomName.ToUpper().Replace(" ", "").Equals(target.ToUpper().Replace(" ", ""))
                            ).AsNoTracking()
                       .FirstOrDefault();
                }
                catch (Exception e)
                {
                    loggingServices.Write(Log, e.Message, LogLevel.Error, e);
                }
            }

            //return id;

            return endPoint;

        }

        public static bool GetEndPointStatus(string target)
        {
            bool rtn = false;
            byte[] tmp = null;
            using (var db = new IOTOI.Model.Db.Context())
            {
                try
                {
                    tmp = (
                            from ep in db.ZigBeeEndPoint
                            join zc in db.ZigBeeInCluster on ep.Id equals zc.ParentId
                            join zca in db.ZigBeeInClusterAttribute on zc.Id equals zca.ParentId
                            where (ep.CustomName != null && ep.CustomName.ToUpper().Replace(" ", "") == target.ToUpper().Replace(" ", "") && zca.ZigBeeType == 16)
                            select zca.AttrValue).FirstOrDefault();

                    rtn = BitConverter.ToBoolean(tmp, 0);
                }
                catch (Exception e)
                {
                    loggingServices.Write(Log, e.Message, LogLevel.Error, e);
                }
            }

            return rtn;

        }
        #endregion

        #region InitTable
        private static void InitTable()
        {
            using (var db = new IOTOI.Model.Db.Context())
            {
                try
                {
                    List<Model.ZigBee.ZigBeeEndDevice> ZigBeeEndDeviceList = db.ZigBeeEndDevice.ToList();

                    //result = db.ZigBeeEndDevice
                    //    .Include(b => b.EndPoints)
                    //        .ThenInclude(ba => ba.ZigBeeInClusters)

                    foreach (Model.ZigBee.ZigBeeEndDevice device in ZigBeeEndDeviceList)
                    {
                        ZigBeeEndDeviceList[ZigBeeEndDeviceList.IndexOf(device)].IsConnected = false;
                    }

                    List<Model.ZigBee.ZigBeeEndPoint> ZigBeeEndPointList = db.ZigBeeEndPoint.ToList();
                    foreach (Model.ZigBee.ZigBeeEndPoint endPoint in ZigBeeEndPointList)
                    {
                        ZigBeeEndPointList[ZigBeeEndPointList.IndexOf(endPoint)].IsActivated = false;
                    }

                    if (ZigBeeEndDeviceList.Count > 0)
                    {
                        db.ZigBeeEndDevice.UpdateRange(ZigBeeEndDeviceList);
                        db.ZigBeeEndPoint.UpdateRange(ZigBeeEndPointList);
                        db.SaveChanges();
                    }
                    //db.ZigBeeEndDevice.RemoveRange(db.ZigBeeEndDevice.ToList());
                    //db.SaveChanges();
                }
                catch (Exception e)
                {
                    loggingServices.Write(Log, e.Message, LogLevel.Error, e);
                }

            }
        }
        #endregion

        #region OnOffToggle()
        private static bool OnOffToggle(Model.ZigBee.ZigBeeEndPoint endPoint, string commandType)
        {
            bool rtn = false;
            if (ZigBeeAdapter != null)
            {
                rtn = ZigBeeAdapter.OnOffToggle(endPoint.MacAddress, endPoint.EpNum, commandType);
            }
            return rtn;
        }
        #endregion

        #region ManagementLeave()
        private static bool ManagementLeave(Model.ZigBee.ZigBeeEndDevice endDevice)
        {
            bool rtn = false;
            if (ZigBeeAdapter != null)
            {
                rtn = ZigBeeAdapter.SendManagementLeave(endDevice.MacAddress);
            }
            return rtn;
        }
        #endregion

        #region GetReturnData
        public static void GetReturnData(ValueSet message, ref ValueSet returnData)
        {
            string command = message["Command"] as string;

            switch (command.ToUpper())
            {
                case "ZIGBEESTATUS":
                    {
                        returnData.Add("Result", IsZigBeeStatus);
                    }
                    break;
                case "DISCOVER":
                    {
                        StartDeviceDiscovery();
                        returnData.Add("Status", "OK");
                    }
                    break;
                case "GETDISCOVERRUNNING":
                    {
                        bool rst = IsRunningDiscoverDevices();
                        //loggingServices.Write(Log, rst.ToString(), LogLevel.Debug);
                        returnData.Add("Result", rst);
                        returnData.Add("Status", "OK");
                    }
                    break;
                case "GETENDDEVICES":
                    {
                        string result = "";
                        try
                        {
                            result = JsonConvert.SerializeObject(GetEndDevices());
                        }
                        catch (Exception e)
                        {
                            loggingServices.Write(Log, e.Message, LogLevel.Error, e);
                        }
                        returnData.Add("Result", result);
                        returnData.Add("Status", "OK");
                    }
                    break;
                case "SETENDDEVICES":
                    {
                        List<string> result = null;
                        try
                        {
                            result = SetEndDevices(JsonConvert.DeserializeObject<List<ValueSet>>(message["Param"].ToString()));
                        }
                        catch (Exception e)
                        {
                            loggingServices.Write(Log, e.Message, LogLevel.Error, e);
                        }
                        returnData.Add("Result", JsonConvert.SerializeObject(result));
                        returnData.Add("Status", "OK");
                    }
                    break;
                case "ONOFFTOGGLE":
                    {
                        string result = "OnOffToggle Success";
                        string status = "OK";

                        Model.ZigBee.ZigBeeEndPoint endPoint = JsonConvert.DeserializeObject<Model.ZigBee.ZigBeeEndPoint>(message["endPoint"].ToString());

                        if (endPoint != null)
                        {
                            if (!OnOffToggle(endPoint, "OnOffToggle"))
                            {
                                result = "Failed to On/Off Toggle";
                                status = "NG";
                            }
                        }
                        else
                        {
                            result = "EndPoint is Null";
                            status = "NG";
                        }
                        returnData.Add("Result", result);
                        returnData.Add("Status", status);
                    }
                    break;
                case "PLUGPOWERON":
                    {
                        string result = "Power On Success";
                        string status = "OK";

                        Model.ZigBee.ZigBeeEndPoint endPoint = JsonConvert.DeserializeObject<Model.ZigBee.ZigBeeEndPoint>(message["endPoint"].ToString());

                        if (endPoint != null)
                        {
                            if (!OnOffToggle(endPoint, "PowerOn"))
                            {
                                result = "Failed to Power On";
                                status = "NG";
                            }
                        }
                        else
                        {
                            result = "EndPoint is Null";
                            status = "NG";
                        }
                        returnData.Add("Result", result);
                        returnData.Add("Status", status);
                    }
                    break;
                case "PLUGPOWEROFF":
                    {
                        string result = "Power Off Success";
                        string status = "OK";
                        Model.ZigBee.ZigBeeEndPoint endPoint = JsonConvert.DeserializeObject<Model.ZigBee.ZigBeeEndPoint>(message["endPoint"].ToString());

                        if (endPoint != null)
                        {
                            if (!OnOffToggle(endPoint, "PowerOff"))
                            {
                                result = "Failed to Power Off";
                                status = "NG";
                            }
                        }
                        else
                        {
                            result = "EndPoint is Null";
                            status = "NG";
                        }
                        returnData.Add("Result", result);
                        returnData.Add("Status", status);
                    }
                    break;
                case "MANAGEMENTLEAVE":
                    {
                        string result = "Management Leave Success";
                        string status = "OK";
                        Model.ZigBee.ZigBeeEndDevice endDevice = JsonConvert.DeserializeObject<Model.ZigBee.ZigBeeEndDevice>(message["endDevice"].ToString());

                        if (endDevice != null)
                        {
                            if (!ManagementLeave(endDevice))
                            {
                                result = "Failed to Management Leave";
                                status = "NG";
                            }
                        }
                        else
                        {
                            result = "EndDevice is Null";
                            status = "NG";
                        }
                        returnData.Add("Result", result);
                        returnData.Add("Status", status);
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
    }
}
