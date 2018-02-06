// Copyright (c) 2015, Microsoft Corporation
//
// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.
//
// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF OR
// IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using BridgeRT;
using System.Diagnostics;
using System.Threading;

using IOTOI.Model.ZigBee;
using IOTOI.Model.Db;
using IOTOI.Model.LoggingServices;
using Windows.Foundation.Collections;

namespace ZigbeeAdapterLib
{
    public sealed class Adapter : IAdapter
    {
        static LoggingServices loggingServices = new LoggingServices();
        private const uint ERROR_SUCCESS = 0;
        private const uint ERROR_INVALID_HANDLE = 6;
        private const uint ERROR_INVALID_DATA = 13;
        private const uint ERROR_INVALID_PARAMETER = 87;
        private const uint ERROR_NOT_SUPPORTED = 50;

        public string Vendor { get; }

        public string AdapterName { get; }

        public string Version { get; }

        public string ExposedAdapterPrefix { get; }

        public string ExposedApplicationName { get; }

        public Guid ExposedApplicationGuid { get; }

        public IList<IAdapterSignal> Signals { get; }

        // A map of signal handle (object's hash code) and related listener entry
        private struct SIGNAL_LISTENER_ENTRY
        {
            // The signal object
            internal IAdapterSignal Signal;

            // The listener object
            internal IAdapterSignalListener Listener;

            //
            // The listener context that will be
            // passed to the signal handler
            //
            internal object Context;
        }
        private Dictionary<int, IList<SIGNAL_LISTENER_ENTRY>> m_signalListeners;

        // ZigBee command used to get new device notification, reportable attributes
        internal readonly DeviceAnnce m_deviceAnnonce = DeviceAnnce.Instance;
        internal readonly ZclReportAttributes m_reportAttributes = ZclReportAttributes.Instance;
        internal readonly ZclServerCommandHandler m_zclServerCommandHandler = ZclServerCommandHandler.Instance;

        public Adapter()
        {
            Windows.ApplicationModel.Package package = Windows.ApplicationModel.Package.Current;
            Windows.ApplicationModel.PackageId packageId = package.Id;
            Windows.ApplicationModel.PackageVersion versionFromPkg = packageId.Version;

            this.Vendor = AdapterHelper.ADAPTER_VENDOR;
            this.AdapterName = AdapterHelper.ADAPTER_NAME;

            // the adapter prefix must be something like "com.mycompany" (only alpha num and dots)
            // it is used by the Device System Bridge as root string for all services and interfaces it exposes
            this.ExposedAdapterPrefix = AdapterHelper.ADAPTER_DOMAIN + "." + this.Vendor.ToLower();
            this.ExposedApplicationGuid = Guid.Parse(AdapterHelper.ADAPTER_APPLICATION_GUID);

            if (null != package && null != packageId)
            {
                this.ExposedApplicationName = packageId.Name;
                this.Version = versionFromPkg.Major.ToString() + "." +
                               versionFromPkg.Minor.ToString() + "." +
                               versionFromPkg.Revision.ToString() + "." +
                               versionFromPkg.Build.ToString();
            }
            else
            {
                this.ExposedApplicationName = AdapterHelper.ADAPTER_DEFAULT_APPLICATION_NAME;
                this.Version = AdapterHelper.ADAPTER_DEFAULT_VERSION;
            }

            try
            {
                this.Signals = new List<IAdapterSignal>();
                this.m_signalListeners = new Dictionary<int, IList<SIGNAL_LISTENER_ENTRY>>();
            }
            catch (OutOfMemoryException ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }

        public uint SetConfiguration([ReadOnlyArray] byte[] ConfigurationData)
        {
            // TODO: add configuration, e.g.: VID/PID of ZigBee dongle
            return ERROR_NOT_SUPPORTED;
        }

        public uint GetConfiguration(out byte[] ConfigurationDataPtr)
        {
            // TODO: add configuration
            ConfigurationDataPtr = null;

            return ERROR_NOT_SUPPORTED;
        }

        public uint Initialize()
        {
            try
            {
                m_xBeeModule.Initialize(out m_adapter);
            }
            catch (Exception ex)
            {
                return (uint)ex.HResult;
            }

            // discover ZigBee device
            CreateSignals();
            StartDeviceDiscovery();

            // add new device and change of attribute value notifications
            m_deviceAnnonce.OnReception += AddDeviceInDeviceList;
            m_reportAttributes.OnReception += ZclReportAttributeReception;
            m_xBeeModule.AddXZibBeeNotification(m_deviceAnnonce);
            m_xBeeModule.AddXZibBeeNotification(m_reportAttributes);
            m_xBeeModule.AddXZibBeeNotification(m_zclServerCommandHandler);

            return ERROR_SUCCESS;
        }

        public uint Shutdown()
        {
            m_xBeeModule.Shutdown();

            return ERROR_SUCCESS;
        }

        public uint EnumDevices(
            ENUM_DEVICES_OPTIONS Options,
            out IList<IAdapterDevice> DeviceListPtr,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            DeviceListPtr = new List<IAdapterDevice>();

            // Add all end points in all ZigBee devices to the list
            // if device list not locked 
            // note that:
            // - an endpoint is a device for BridgeRT
            // - endpoints will be notified as ZigBee devices arrive 
            if (Monitor.TryEnter(m_deviceMap))
            {
                try
                {
                    foreach (var zigBeeDeviceItem in m_deviceMap)
                    {
                        foreach (var endPointItem in zigBeeDeviceItem.Value.EndPointList)
                        {
                            DeviceListPtr.Add(endPointItem.Value);
                        }
                    }
                }
                finally
                {
                    // use try/finally to ensure m_deviceList is unlocked in every case
                    Monitor.Exit(m_deviceMap);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint GetProperty(
            IAdapterProperty Property,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            // sanity check
            if (Property == null)
            {
                return ERROR_INVALID_PARAMETER;
            }

            // cast back IAdapterProperty to ZclCluster
            ZclCluster cluster = (ZclCluster)Property;

            // read all attributes for the attribute
            foreach (var item in cluster.InternalAttributeList)
            {
                var attribute = item.Value;
                object value;
                if (!attribute.Read(out value))
                {
                    // give up at 1st read error
                    return (uint)ZclHelper.ZigBeeStatusToHResult(attribute.Status);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint SetProperty(
            IAdapterProperty Property,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            // sanity check
            if (Property == null)
            {
                return ERROR_INVALID_PARAMETER;
            }

            // cast back IAdapterProperty to ZclCluster
            ZclCluster cluster = (ZclCluster)Property;

            // write new value for all attributes
            // note that it is assumed that BridgeRT has set the new value
            // for each attribute
            foreach (var item in cluster.InternalAttributeList)
            {
                var attribute = item.Value;
                if (attribute.Write(attribute.Value.Data))
                {
                    // give up at 1st write error
                    return (uint)ZclHelper.ZigBeeStatusToHResult(attribute.Status);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint GetPropertyValue(
            IAdapterProperty Property,
            string AttributeName,
            out IAdapterValue ValuePtr,
            out IAdapterIoRequest RequestPtr)
        {
            ValuePtr = null;
            RequestPtr = null;

            // sanity check
            if (Property == null)
            {
                return ERROR_INVALID_PARAMETER;
            }

            // cast back IAdapterProperty to ZclCluster
            ZclCluster cluster = (ZclCluster)Property;

            // look for the attribute
            foreach (var item in cluster.InternalAttributeList)
            {
                var attribute = item.Value;
                if (attribute.Value.Name == AttributeName)
                {
                    object value;
                    if (attribute.Read(out value))
                    {
                        ValuePtr = attribute.Value;
                        return ERROR_SUCCESS;
                    }
                    else
                    {
                        return (uint)ZclHelper.ZigBeeStatusToHResult(attribute.Status);
                    }
                }
            }

            return ERROR_NOT_SUPPORTED;
        }

        public uint SetPropertyValue(
            IAdapterProperty Property,
            IAdapterValue Value,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            // sanity check
            if (Property == null)
            {
                return ERROR_INVALID_PARAMETER;
            }

            // cast back IAdapterProperty to ZclCluster
            ZclCluster cluster = (ZclCluster)Property;

            // look for the attribute and write new data
            foreach (var item in cluster.InternalAttributeList)
            {
                var attribute = item.Value;
                if (attribute.Value.Name == Value.Name)
                {
                    if (attribute.Write(Value.Data))
                    {
                        return ERROR_SUCCESS;
                    }
                    else
                    {
                        return (uint)ZclHelper.ZigBeeStatusToHResult(attribute.Status);
                    }
                }
            }

            return ERROR_NOT_SUPPORTED;
        }

        public uint CallMethod(
            IAdapterMethod Method,
            out IAdapterIoRequest RequestPtr)
        {
            RequestPtr = null;

            // sanity check
            if (Method == null)
            {
                return ERROR_INVALID_PARAMETER;
            }

            // IAdapterMethod is either a ZclCommand or a ManagementLeave command,
            // cast back IAdapterMethod to ZclCommand first then to ManagementLeave if cast failed 
            try
            {
                var command = (ZclCommand)Method;
                command.Send();
                return (uint)command.HResult;
            }
            catch (InvalidCastException e)
            {
                // send the leave command and remove devices (hence all end points of the ZigBee device)
                //
                // Note that ManagementLeave is THE unique ZdoCommand exposed to AllJoyn
                //
                var command = (ManagementLeave)Method;
                command.Send();

                if (command.ZigBeeStatus != ZdoHelper.ZDO_NOT_SUPPORTED)
                {
                    // async device removal
                    Task.Run(() => RemoveDevice(command.Device));
                }

                return (uint)command.HResult;
            }
        }

        public uint RegisterSignalListener(
            IAdapterSignal Signal,
            IAdapterSignalListener Listener,
            object ListenerContext)
        {
            // sanity check
            if (Signal == null || Listener == null)
            {
                return ERROR_INVALID_PARAMETER;
            }

            int signalHashCode = Signal.GetHashCode();

            SIGNAL_LISTENER_ENTRY newEntry;
            newEntry.Signal = Signal;
            newEntry.Listener = Listener;
            newEntry.Context = ListenerContext;

            lock (m_signalListeners)
            {
                if (m_signalListeners.ContainsKey(signalHashCode))
                {
                    m_signalListeners[signalHashCode].Add(newEntry);
                }
                else
                {
                    var newEntryList = new List<SIGNAL_LISTENER_ENTRY> { newEntry };
                    m_signalListeners.Add(signalHashCode, newEntryList);
                }
            }

            return ERROR_SUCCESS;
        }

        public uint UnregisterSignalListener(
            IAdapterSignal Signal,
            IAdapterSignalListener Listener)
        {
            return ERROR_SUCCESS;
        }

        private uint CreateSignals()
        {
            // create device arrival signal and add it to list
            AdapterSignal deviceArrival = new AdapterSignal(Constants.DEVICE_ARRIVAL_SIGNAL);
            deviceArrival.AddParam(Constants.DEVICE_ARRIVAL__DEVICE_HANDLE);
            Signals.Add(deviceArrival);

            // create device removal signal and add it to list
            AdapterSignal deviceRemoval = new AdapterSignal(Constants.DEVICE_REMOVAL_SIGNAL);
            deviceRemoval.AddParam(Constants.DEVICE_REMOVAL__DEVICE_HANDLE);
            Signals.Add(deviceRemoval);

            return ERROR_SUCCESS;
        }

        internal void NotifyBridgeRT(ZigBeeEndPoint endPoint, string signalName, string paramName)
        {
            // find device arrival signal in list
            var deviceArrival = Signals.OfType<AdapterSignal>().FirstOrDefault(s => s.Name == signalName);
            if (deviceArrival == null)
            {
                // no device arrival signal
                return;
            }

            // set parameter value
            var param = deviceArrival.Params.FirstOrDefault(p => p.Name == paramName);
            if (param == null)
            {
                // signal doesn't have the expected parameter
                return;
            }
            param.Data = endPoint;

            NotifySignalListeners(deviceArrival);
        }

        internal void NotifySignalListeners(IAdapterSignal signal)
        {
            int signalHashCode = signal.GetHashCode();

            IList<SIGNAL_LISTENER_ENTRY> listenerList = null;
            lock (m_signalListeners)
            {
                if (m_signalListeners.ContainsKey(signalHashCode))
                {
                    // make a local copy of the listener list
                    listenerList = m_signalListeners[signalHashCode].ToArray();
                }
                else
                {
                    // can't do anything 
                    return;
                }

            }

            // call out event handlers out of the lock to avoid
            // deadlock risk
            foreach (SIGNAL_LISTENER_ENTRY entry in listenerList)
            {
                IAdapterSignalListener listener = entry.Listener;
                object listenerContext = entry.Context;
                listener.AdapterSignalHandler(signal, listenerContext);
            }
        }

        private void RemoveDevice(ZigBeeDevice zigBeeDevice)
        {
            // remove device from list (it will be garbaged collect later)
            lock (m_deviceMap)
            {
                m_deviceMap.Remove(zigBeeDevice.MacAddress);
            }

            // remove server commands that belong to that device
            m_zclServerCommandHandler.RemoveCommands(zigBeeDevice);

            // notify AllJoyn/BridgeRT
            // 
            // note that a ZigBee endpoint is a exposed as a device on AllJoyn
            // => BridgeRT should be notified of removal of each enpoint of the removed ZigBee device
            foreach (var endPoint in zigBeeDevice.EndPointList)
            {
                NotifyBridgeRT(endPoint.Value, Constants.DEVICE_REMOVAL_SIGNAL, Constants.DEVICE_REMOVAL__DEVICE_HANDLE);
            }
        }

        private XBeeModule m_xBeeModule = new XBeeModule();
        internal XBeeModule XBeeModule
        {
            get { return m_xBeeModule; }
        }

        private ZigBeeDevice m_adapter = null;
        private ZclClusterFactory m_zclClusterFactory = ZclClusterFactory.Instance;
        private ZigBeeProfileLibrary m_zigBeeProfileLibrary = ZigBeeProfileLibrary.Instance;
        private readonly Dictionary<UInt64, ZigBeeDevice> m_deviceMap = new Dictionary<UInt64, ZigBeeDevice>();
        internal Dictionary<UInt64, ZigBeeDevice> DeviceList
        {
            get { return m_deviceMap; }
        }


        public void StartDeviceDiscovery()
        {
            // async discovery process
            Task.Run(() => {
                DiscoverDevices();
                //To DO Insert All Device Info
            });
        }


        public bool GetRunningDiscoverDevices()
        {
            return IsRunningDiscoverDevices;
        }

        internal bool IsRunningDiscoverDevices = false;

        private void DiscoverDevices()
        {

            ManagementLQI lqiCommand = new ManagementLQI();

            // lock device list and clear it
            lock (m_deviceMap)
            {
                m_deviceMap.Clear();
            }

            // get direct neighbor and add them in list
            //
            // note: for now, only deal with direct neighbor (no hop)
            //       going through ZigBee network nodes and graph will come later
            lqiCommand.GetNeighbors(m_xBeeModule, m_adapter);
            IsRunningDiscoverDevices = true;

            Debug.WriteLine("NeighborList = " + lqiCommand.NeighborList.Count);
            Debug.WriteLine("IsRunningDiscoverDevices = " + IsRunningDiscoverDevices);

            List<Task> list = new List<Task>();

            foreach (var deviceDescriptor in lqiCommand.NeighborList)
            {
                //ZigBeeDevice tempDevice = new ZigBeeDevice(deviceDescriptor.networkAddress, deviceDescriptor.macAddress, deviceDescriptor.isEndDevice);
                Task t = new Task(() => AddDeviceInDeviceList(deviceDescriptor.networkAddress, deviceDescriptor.macAddress, deviceDescriptor.isEndDevice));
                list.Add(t);
                t.Start();
            }

            Task.WaitAll(list.ToArray());


            IsRunningDiscoverDevices = false;

            Debug.WriteLine("IsRunningDiscoverDevices = " + IsRunningDiscoverDevices);
        }
        private void AddDeviceInDeviceList(UInt16 networkAddress, UInt64 macAddress, bool isEndDevice)
        {
            Debug.WriteLine("AddDeviceInDeviceList Start " + macAddress);
            try
            {
                bool deviceFound = false;
                bool addDeviceInList = false;
                ZigBeeDevice device = null;
                
                Logger.TraceDevice(networkAddress, macAddress);

                lock (m_deviceMap)
                {
                    deviceFound = m_deviceMap.TryGetValue(macAddress, out device);
                }

                if (!deviceFound)
                {
                    // the device isn't in the list yet
                    device = new ZigBeeDevice(networkAddress, macAddress, isEndDevice);

                    // get end points and supported clusters
                    ActiveEndPoints activeEndPointsCommand = new ActiveEndPoints();
                    activeEndPointsCommand.GetEndPoints(m_xBeeModule, device);
                    
                    foreach (var endPointId in activeEndPointsCommand.EndPointList)
                    {
                        SimpleDescriptor descriptor = new SimpleDescriptor();
                        #region SOURCE_ENDPOINT => 추가해주지 않으면 커맨드가 정상적으로 날아가지 않는 현상으로인해 수정 - by 김진엽
                        activeEndPointsCommand.SetSOURCE_ENDPOINT(new byte[] { endPointId });
                        #endregion
                        
                        loggingServices.WriteLine<SimpleDescriptor>("[" + macAddress.ToString() + "][" + endPointId.ToString() + "] + Descript cluster list Start");
                        descriptor.GetDescriptor(m_xBeeModule, device, endPointId);
                        loggingServices.WriteLine<SimpleDescriptor>("[" + macAddress.ToString() + "][" + endPointId.ToString() + "] + Descript cluster list End");
                        Logger.TraceDeviceDetailed(device.MacAddress, endPointId, descriptor);

                        foreach (var clusterId in descriptor.InClusterList)
                        {
                            loggingServices.WriteLine<SimpleDescriptor>("[" + macAddress.ToString() + "][" + endPointId.ToString() + "][InCluster][" + clusterId.ToString() + "]" + m_zclClusterFactory.IsClusterSupported(clusterId));
                            if (m_zclClusterFactory.IsClusterSupported(clusterId) &&
                                device.AddClusterToEndPoint(true, endPointId, descriptor.ProfileId, descriptor.DeviceId, clusterId, this))
                            {
                                // only add device in list if at least 1 cluster is supported
                                addDeviceInList = true;
                            }
                        }
                        foreach (var clusterId in descriptor.OutClusterList)
                        {
                            loggingServices.WriteLine<SimpleDescriptor>("[" + macAddress.ToString() + "][" + endPointId.ToString() + "][OutCluster][" + clusterId.ToString() + "]" + m_zclClusterFactory.IsClusterSupported(clusterId));
                            if (m_zclClusterFactory.IsClusterSupported(clusterId) &&
                                device.AddClusterToEndPoint(false, endPointId, descriptor.ProfileId, descriptor.DeviceId, clusterId, this))
                            {
                                // only add device in list if at least 1 cluster is supported
                                addDeviceInList = true;
                            }
                        }
                    }
                }
                else
                {
                    // the device is already in list so just refresh its network address.
                    // note that mac address will never change but network address can, e.g.: if end device connects to another router
                    device.NetworkAddress = networkAddress;
                }



                // add device in list if necessary
                if (addDeviceInList)
                {
                    lock (m_deviceMap)
                    {
                        // add device in list if it has not been added between beginning of this routine and now
                        ZigBeeDevice tempDevice = null;
                        if (!m_deviceMap.TryGetValue(device.MacAddress, out tempDevice))
                        {
                            m_deviceMap.Add(device.MacAddress, device);
                            //Insert or update device
                            IOTOI.Model.ZigBee.ZigBeeEndDevice deviceModel = null;
                            using (var db = new Context())
                            {
                                deviceModel = db.ZigBeeEndDevice.Find(device.MacAddress);
                            }
                            if (deviceModel == null)
                            {
                                InsertDevice(device);
                            }
                            else
                            {
                                UpdateConnectActiveStatus(deviceModel, device);
                            }
                            
                        }
                        else
                        {
                            // device has been added => update network address of already existing device
                            // give up with device that has been created and use the already existing device instead
                            tempDevice.NetworkAddress = device.NetworkAddress;
                            tempDevice.NetworkAddress = device.NetworkAddress;
                            device = tempDevice;
                            UpdateNetworkAddress(device);
                        }
                    }
                }

                // notify devices to bridgeRT
                // note end points are devices as far as BridgeRT is concerned
                foreach (var endpoint in device.EndPointList)
                {
                    NotifyBridgeRT(endpoint.Value, Constants.DEVICE_ARRIVAL_SIGNAL, Constants.DEVICE_ARRIVAL__DEVICE_HANDLE);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }


            Debug.WriteLine("AddDeviceInDeviceList End " + macAddress);
        }

        private const string TypeZigBee = "ZIGBEE";

        private void InsertDevice(ZigBeeDevice device)
        {
            try
            {
                //CommaxIot.Model.ZigBee.ZigBeeEndDevice endDeviceModel = new CommaxIot.Model.ZigBee.ZigBeeEndDevice();
                ZigBeeEndDevice endDeviceModel = new ZigBeeEndDevice();
                
                endDeviceModel.MacAddress = device.MacAddress;
                endDeviceModel.NetworkAddress = device.NetworkAddress;
                endDeviceModel.Name = device.Name;
                endDeviceModel.IsConnected = true;

                endDeviceModel.EndPoints = GetEndPointListModel(device.EndPointList.Values, device.MacAddress);

                ZigBeeSaveChanges("insert", endDeviceModel, out bool result);
                if (result)
                {
                    loggingServices.WriteLine<Adapter>("[" + device.MacAddress + "]InsertDevice Success");
                }
            }
            catch(Exception e)
            {
             
                loggingServices.WriteLine<Adapter>("[" + device.MacAddress + "]InsertDevice Failed");
                loggingServices.WriteLine<Adapter>("[" + device.MacAddress + "]error ==> " + e.Message);
            }            
        }

        private List<IOTOI.Model.ZigBee.ZigBeeEndPoint> GetEndPointListModel(Dictionary<byte, ZigBeeEndPoint>.ValueCollection zigBeeEndPoint, ulong macAddress)
        {
            List<IOTOI.Model.ZigBee.ZigBeeEndPoint> endPointListModel = new List<IOTOI.Model.ZigBee.ZigBeeEndPoint>();
            foreach (ZigBeeEndPoint endPoint in zigBeeEndPoint)                                                                                                 
            {
                IOTOI.Model.ZigBee.ZigBeeEndPoint endPointModel = new IOTOI.Model.ZigBee.ZigBeeEndPoint();

                endPointModel.MacAddress = macAddress;
                endPointModel.EpNum = endPoint.Id;
                endPointModel.DeviceId = endPoint.DeviceId;
                endPointModel.CommanProfileId = endPoint.CommandProfileId;
                endPointModel.Vendor = endPoint.Vendor;
                endPointModel.Model = endPoint.Model;
                endPointModel.Name = endPoint.Name;
                endPointModel.ProtocolTypeId = GetProtocol();
                endPointModel.IsActivated = true;
                
                endPointModel.ZigBeeInClusters = GetInClusterListModel(endPoint.inClusters.Values, endPoint.Properties);
                endPointModel.ZigBeeOutClusters = GetOutClusterListModel(endPoint.outClusters.Values, endPoint.Properties);

                endPointListModel.Add(endPointModel);
            }
            return endPointListModel;
        }

        private List<IOTOI.Model.ZigBee.ZigBeeInCluster> GetInClusterListModel(
            Dictionary<ushort, ZclCluster>.ValueCollection inClusterList, 
            IList<BridgeRT.IAdapterProperty> properties)
        {
            List<IOTOI.Model.ZigBee.ZigBeeInCluster> inClusterListModel = new List<IOTOI.Model.ZigBee.ZigBeeInCluster>();

            foreach (ZclCluster cluster in properties)
            {
                IOTOI.Model.ZigBee.ZigBeeInCluster inClusterModel = new IOTOI.Model.ZigBee.ZigBeeInCluster();

                if (inClusterList.Contains(cluster))                                                                                                                
                {
                    inClusterModel.ClusterId = cluster.Id;
                    inClusterModel.Name = cluster.Name;

                    inClusterModel.ZigBeeInClusterAttributes = GetInClusterAttributeListModel(cluster);                                                             

                    inClusterListModel.Add(inClusterModel);
                }
            }
            return inClusterListModel;
        }
        
        private List<IOTOI.Model.ZigBee.ZigBeeInClusterAttribute> GetInClusterAttributeListModel(ZclCluster cluster)
        {
            List<IOTOI.Model.ZigBee.ZigBeeInClusterAttribute> inClusterAttributeListModel = new List<IOTOI.Model.ZigBee.ZigBeeInClusterAttribute>();  
            
            foreach (ZclAttribute attribute in cluster.InternalAttributeList.Values)                                                                                                   
            {
                IOTOI.Model.ZigBee.ZigBeeInClusterAttribute inClusterAttributeModel = new IOTOI.Model.ZigBee.ZigBeeInClusterAttribute();                
                uint rtn = GetPropertyValue(cluster, attribute.Value.Name, out IAdapterValue value, out IAdapterIoRequest request);                             
                if (rtn == ERROR_SUCCESS)
                {
                    ZclValue zclValue = (ZclValue)value;
                    inClusterAttributeModel.Name = zclValue.Name;
                    inClusterAttributeModel.AttrValue = zclValue.ToByteBuffer();
                    inClusterAttributeModel.ZigBeeType = zclValue.Type;

                    inClusterAttributeListModel.Add(inClusterAttributeModel);
                }
                else
                {
                    //loggingServices.WriteLine<Adapter>("Mac = [" + device.MacAddress + "], Ep = [" + endPoint.Id + "], cluster = [" + cluster.Id + "] + attribute = [" + attribute.Value.Name + "] ==> Fail to get attribute object");
                    loggingServices.WriteLine<Adapter>("cluster = [" + cluster.Id + "] + attribute = [" + attribute.Value.Name + "] ==> Fail to get attribute object");
                }
            }
            return inClusterAttributeListModel;
        }

        private List<IOTOI.Model.ZigBee.ZigBeeOutCluster> GetOutClusterListModel(
            Dictionary<ushort, ZclCluster>.ValueCollection outClusterList, 
            IList<BridgeRT.IAdapterProperty> properties)
        {
            List<IOTOI.Model.ZigBee.ZigBeeOutCluster> outClusterListModel = new List<IOTOI.Model.ZigBee.ZigBeeOutCluster>();

            foreach (ZclCluster cluster in properties)
            {
                IOTOI.Model.ZigBee.ZigBeeOutCluster outClusterModel = new IOTOI.Model.ZigBee.ZigBeeOutCluster();

                if (outClusterList.Contains(cluster))                                                                                                                    
                {
                    outClusterModel.ClusterId = cluster.Id;
                    outClusterModel.Name = cluster.Name;

                    outClusterModel.ZigBeeOutClusterAttributes = GetOutClusterAttributeListModel(cluster);                                                               

                    outClusterListModel.Add(outClusterModel);
                }
            }

            return outClusterListModel;
        }

        private List<IOTOI.Model.ZigBee.ZigBeeOutClusterAttribute> GetOutClusterAttributeListModel(ZclCluster cluster)
        {
            List<IOTOI.Model.ZigBee.ZigBeeOutClusterAttribute> outClusterAttributeListModel = new List<IOTOI.Model.ZigBee.ZigBeeOutClusterAttribute>();       

            foreach (ZclAttribute attribute in cluster.InternalAttributeList.Values)                                                                                                    
            {
                IOTOI.Model.ZigBee.ZigBeeOutClusterAttribute outClusterAttributeModel = new IOTOI.Model.ZigBee.ZigBeeOutClusterAttribute();              
                uint rtn = GetPropertyValue(cluster, attribute.Value.Name, out IAdapterValue value, out IAdapterIoRequest request);                             
                if (rtn == ERROR_SUCCESS)
                {
                    ZclValue zclValue = (ZclValue)value;
                    outClusterAttributeModel.Name = zclValue.Name;
                    outClusterAttributeModel.AttrValue = zclValue.ToByteBuffer();
                    outClusterAttributeModel.ZigBeeType = zclValue.Type;

                    outClusterAttributeListModel.Add(outClusterAttributeModel);
                }
                else
                {
                    //loggingServices.WriteLine<Adapter>("Mac = [" + device.MacAddress + "], Ep = [" + endPoint.Id + "], cluster = [" + cluster.Id + "] + attribute = [" + attribute.Value.Name + "] ==> Fail to get attribute object");
                    loggingServices.WriteLine<Adapter>("cluster = [" + cluster.Id + "] + attribute = [" + attribute.Value.Name + "] ==> Fail to get attribute object");
                }
            }
            return outClusterAttributeListModel;
        }

        private void UpdateConnectActiveStatus(IOTOI.Model.ZigBee.ZigBeeEndDevice deviceModel, ZigBeeDevice zigBeeDevice)
        {
            deviceModel.IsConnected = true;

            ZigBeeSaveChanges("update", deviceModel, out bool result);
            if (!result)
            {
                loggingServices.WriteLine<Adapter>("[" + deviceModel.MacAddress + "]Fail to update device connect status");
            }
            else
            {
                Dictionary<byte, ZigBeeEndPoint>.ValueCollection endPointList = zigBeeDevice.EndPointList.Values;
                IOTOI.Model.ZigBee.ZigBeeEndPoint endPointModel = new IOTOI.Model.ZigBee.ZigBeeEndPoint();
                List<IOTOI.Model.ZigBee.ZigBeeEndPoint> endPointListModel = new List<IOTOI.Model.ZigBee.ZigBeeEndPoint>();
                foreach (ZigBeeEndPoint endPoint in endPointList)
                {
                    using (var db = new Context())
                    {
                        endPointModel = db.ZigBeeEndPoint.SingleOrDefault(b => b.MacAddress == zigBeeDevice.MacAddress && b.EpNum == endPoint.Id);
                    }
                    if (endPointModel != null)
                    {
                        endPointModel.IsActivated = true;
                        ZigBeeSaveChanges("update", endPointModel, out result);
                        if (!result)
                        {
                            loggingServices.WriteLine<Adapter>("[" + deviceModel.MacAddress + "]Fail to update EndPoint activate status");
                        }
                    }
                    else
                    {
                        endPointModel = new IOTOI.Model.ZigBee.ZigBeeEndPoint();
                        endPointModel.MacAddress = zigBeeDevice.MacAddress;
                        endPointModel.EpNum = endPoint.Id;
                        endPointModel.DeviceId = endPoint.DeviceId;
                        endPointModel.CommanProfileId = endPoint.CommandProfileId;
                        endPointModel.Name = endPoint.Name;
                        endPointModel.ProtocolTypeId = GetProtocol();
                        endPointModel.IsActivated = true;

                        endPointModel.ZigBeeInClusters = GetInClusterListModel(endPoint.inClusters.Values, endPoint.Properties);
                        endPointModel.ZigBeeOutClusters = GetOutClusterListModel(endPoint.outClusters.Values, endPoint.Properties);

                        ZigBeeSaveChanges("insert", endPointModel, out result);
                        if (!result)
                        {
                            loggingServices.WriteLine<Adapter>("[" + deviceModel.MacAddress + "]["+ endPointModel.EpNum+"]Fail to insert new EndPoint");
                        }
                    }
                }
            }
        }

         #region UpdateNetworkAddress()
        private void UpdateNetworkAddress(ZigBeeDevice zigBeeDevice)
        {
            IOTOI.Model.ZigBee.ZigBeeEndDevice endDevice;

            using (var db = new Context())
            {
                endDevice = db.ZigBeeEndDevice.Single(b => b.MacAddress == zigBeeDevice.MacAddress);
                endDevice.NetworkAddress = zigBeeDevice.NetworkAddress;
                endDevice.IsConnected = true;
            }

            ZigBeeSaveChanges("update", endDevice, out bool result);
            if (result)
            {
                loggingServices.WriteLine<Adapter>("[" + zigBeeDevice.MacAddress + "]UpdateDevice Success");
            }
        }
        #endregion

        #region GetProtocol()
        private int GetProtocol()
        {
            int rtn = 0;

            try
            {
                using (var db = new Context())
                {
                    rtn = db.ProtocolType
                        .Where(b => b.Name == TypeZigBee)
                        .FirstOrDefault().Id;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return rtn;
        }
        #endregion

        private void ZclReportAttributeReception(ZclReportAttributes.SOURCE_INFO deviceInfo, UInt16 attributeId, object newValue)
        {
            ZigBeeDevice device = null;
            ZigBeeEndPoint endPoint = null;
            ZclCluster cluster = null;
            ZclAttribute attribute = null;

            // look for corresponding ZigBee device
            lock (m_deviceMap)
            {
                if (!m_deviceMap.TryGetValue(deviceInfo.macAddress, out device))
                {
                    // unknown device => do nothing
                    return;
                }
            }

            // look for corresponding end point
            if (!device.EndPointList.TryGetValue(deviceInfo.endpointId, out endPoint))
            {
                // unknown end point => do nothing
                return;
            }

            // look for corresponding cluster
            cluster = endPoint.GetCluster(deviceInfo.clusterId);
            if (cluster == null)
            {
                // unknown cluster => do nothing
                return;
            }

            // look for the corresponding attribute
            if (!cluster.InternalAttributeList.TryGetValue(attributeId, out attribute)) // 비지니스 로직의 오류로인해 상태값을 가져오지 못하는 현상. 수정함. -- By 김진엽
            {
                // unknown attribute => do nothing
                return;
            }

            // update value
            attribute.Value.Data = newValue;

            // signal value of attribute has changed
            SignalChangeOfAttributeValue(endPoint, cluster, attribute);

            // Database Update to value of changed attribute - By 김진엽
            UpdateAttributeValue(device, endPoint, cluster, attribute);
        }

        internal void SignalChangeOfAttributeValue(ZigBeeEndPoint endPoint, ZclCluster cluster, ZclAttribute attribute)
        {
            // find change of value signal of that end point (end point == bridgeRT device)
            var covSignal = endPoint.Signals.OfType<AdapterSignal>().FirstOrDefault(s => s.Name == Constants.CHANGE_OF_VALUE_SIGNAL);
            if (covSignal == null)
            {
                // no change of value signal
                return;
            }

            // set property and attribute param of COV signal
            // note that 
            // - ZCL cluster correspond to BridgeRT property 
            // - ZCL attribute correspond to BridgeRT attribute 
            var param = covSignal.Params.FirstOrDefault(p => p.Name == Constants.COV__PROPERTY_HANDLE);
            if (param == null)
            {
                // signal doesn't have the expected parameter
                return;
            }
            param.Data = cluster;

            param = covSignal.Params.FirstOrDefault(p => p.Name == Constants.COV__ATTRIBUTE_HANDLE);
            if (param == null)
            {
                // signal doesn't have the expected parameter
                return;
            }
            param.Data = attribute;

            // signal change of value to BridgeRT
            NotifySignalListeners(covSignal);
        }

        public UInt64 MacAddress
        {
            get { return m_adapter.MacAddress; }
        }

        #region sendCommand
        private static int SendCommand(ZclCommand command)
        {
            command.Send();
            
            return command.HResult;
        }
        #endregion

        #region OnOff()
        public bool OnOffToggle(UInt64 MAC_ADDRESS, byte END_POINT, string commandType)
        {
            bool result = false;
            int commandResult = 0;
            Task.Run(() =>
            {
                ZigBeeDevice device = DeviceList[MAC_ADDRESS];
                ZigBeeEndPoint endPoint = device.EndPointList[END_POINT];
                ZclCluster cluster = endPoint.GetCluster(OnOffCluster.CLUSTER_ID);
                byte commandId = 0;

                if ("ONOFFTOGGLE".Equals(commandType.ToUpper()))
                {
                    commandId = OnOffCluster.COMMAND_TOGGLE;
                }
                else if ("POWERON".Equals(commandType.ToUpper()))
                {
                    commandId = OnOffCluster.COMMAND_ON;
                }
                else if ("POWEROFF".Equals(commandType.ToUpper()))
                {
                    commandId = OnOffCluster.COMMAND_OFF;
                }
                var command = cluster.CommandList[commandId];
                commandResult = SendCommand(command);

                if (commandResult!= ZclHelper.ZCL_ERROR_SUCCESS)
                {
                    loggingServices.WriteLine<Adapter>("[" + MAC_ADDRESS + "][" + END_POINT + "] Failed to send command");
                }
            });
            return result;
        }
        #endregion


        public bool SendManagementLeave(ulong macAddress)
        {
            bool result = true;
            string message = "Management Leave Success";
            //IAdapterIoRequest RequestPtr = null;

            try
            {
                IOTOI.Model.ZigBee.ZigBeeEndDevice endDeviceModel = null;
                using (var db = new Context())
                {
                    endDeviceModel = db.ZigBeeEndDevice.Find(macAddress);
                    if (endDeviceModel != null)
                    {
                        db.ZigBeeEndDevice.Remove(endDeviceModel);
                        int trResult = db.SaveChanges();

                        if (trResult < 1)
                        {
                            result = false;
                            message = "[" + macAddress + "]Fail to remove device data.";
                        }
                    }
                }
            }
            finally
            {
                Task.Run(() => {
                    if (DeviceList.TryGetValue(macAddress, out ZigBeeDevice zigBeeDevice))
                    {


                        Dictionary<byte, ZigBeeEndPoint>.ValueCollection endPointList = zigBeeDevice.EndPointList.Values;
                        uint HResult = AdapterHelper.S_OK;
                        foreach (ZigBeeEndPoint endPoint in endPointList)
                        {
                            List<IAdapterMethod> Methods = endPoint.Methods.ToList();
                            IAdapterMethod leave = Methods[Methods.Count() - 1];
                            HResult = CallMethod(leave, out IAdapterIoRequest RequestPtr);
                            if (HResult == AdapterHelper.S_OK)
                            {
                                break;
                            }
                        }
                        if (HResult != AdapterHelper.S_OK)
                        {
                            result = false;
                            message = "[" + zigBeeDevice.MacAddress + "]Management Leave Failed.";

                        }
                    }
                    loggingServices.WriteLine<Adapter>(message);
                });
                
            }
            

            
            return result;
        }

        //#region SendManagementLeave
        //public bool SendManagementLeave(ulong macAddress)
        //{
        //    bool result = true;
        //    string message = "[" + macAddress + "]Success to Managementleave";
        //    if (DeviceList.TryGetValue(macAddress, out ZigBeeDevice zigBeeDevice))
        //    {
        //        ManagementLeave managementLeave = new ManagementLeave(zigBeeDevice);
        //        managementLeave.Send();

        //        int HResult = managementLeave.HResult;
        //        if (HResult == AdapterHelper.S_OK)
        //        {
        //            CommaxIot.Model.ZigBee.ZigBeeEndDevice endDeviceModel = null;
        //            using (var db = new Context())
        //            {
        //                endDeviceModel = db.ZigBeeEndDevice.Find(macAddress);
        //                if (endDeviceModel != null)
        //                {
        //                    db.ZigBeeEndDevice.Remove(endDeviceModel);
        //                    int trResult = db.SaveChanges();

        //                    if(trResult < 1)
        //                    {
        //                        result = false;
        //                        message = "[" + zigBeeDevice.MacAddress + "]Success to ManagementLeave. But Fail to remove device data."; 
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            result = false;
        //            message = "[" + zigBeeDevice.MacAddress + "]Fail to Managementleave. HResult = " + HResult.ToString();
        //        }
        //    }
        //    else
        //    {
        //        result = false;
        //        message = "[" + zigBeeDevice.MacAddress + "]Fail to Managementleave. Unkown Device";
        //    }
        //    loggingServices.WriteLine<Adapter>(message);
        //    return result;
        //}
        //#endregion

        #region UpdateAttributeValue()
        private bool UpdateAttributeValue(ZigBeeDevice zigBeeDevice, ZigBeeEndPoint zigBeeEndPoint, ZclCluster cluster, ZclAttribute attribute)
        {
            IOTOI.Model.ZigBee.ZigBeeEndDevice zigBeeEndDeviceModel;
            IOTOI.Model.ZigBee.ZigBeeEndPoint zigBeeEndPointModel;
            IOTOI.Model.ZigBee.ZigBeeInCluster zigBeeInClusterModel;
            IOTOI.Model.ZigBee.ZigBeeOutCluster zigBeeOutClusterModel;
            IOTOI.Model.ZigBee.ZigBeeInClusterAttribute zigBeeInClusterAttributeModel = new IOTOI.Model.ZigBee.ZigBeeInClusterAttribute();
            IOTOI.Model.ZigBee.ZigBeeOutClusterAttribute zigBeeOutClusterAttributeModel = new IOTOI.Model.ZigBee.ZigBeeOutClusterAttribute();

            bool result = true;

            try
            {
                //TODO: SendTTS작업시 사용할 내용임.
                string endPointName = null;
                bool isOpenBefore = false;
                using (var db = new Context())
                {
                    zigBeeEndDeviceModel = db.ZigBeeEndDevice.Find(zigBeeDevice.MacAddress);

                    if (zigBeeEndDeviceModel != null)
                    {
                        zigBeeEndPointModel = db.ZigBeeEndPoint.Single(b => b.MacAddress == zigBeeEndDeviceModel.MacAddress && b.EpNum == zigBeeEndPoint.Id);
                        zigBeeInClusterModel = db.ZigBeeInCluster.Single(b => b.ParentId == zigBeeEndPointModel.Id && b.ClusterId == cluster.Id);
                        zigBeeInClusterAttributeModel = db.ZigBeeInClusterAttribute.Single(b => b.ParentId == zigBeeInClusterModel.Id && b.Name == attribute.Value.Name);
                        
                        isOpenBefore = BitConverter.ToBoolean(zigBeeInClusterAttributeModel.AttrValue, 0);

                        ZclValue zclValue = (ZclValue)attribute.Value;
                        zigBeeInClusterAttributeModel.AttrValue = zclValue.ToByteBuffer();
                        
                        endPointName = zigBeeEndPointModel.CustomName;  //endPointName 추가
                    }
                    else
                    {
                        result = false;
                    }
                }

                if (result)
                {
                    ZigBeeSaveChanges("update", zigBeeInClusterAttributeModel, out result);
                    
                    if (endPointName == null)
                    {
                        loggingServices.WriteLine<Adapter>("EndPoint Name is Null");
                    }
                    
                    if (attribute.Value.Name == "OnOff" && endPointName != null)
                    {
                        ZclValue zclValue = (ZclValue)attribute.Value;
                        bool isOpen = (bool)zclValue.Data;
                        
                        if(isOpen != isOpenBefore)
                        {
                            string rtnMessage = "The " + endPointName + " is ";
                            rtnMessage += isOpen ? "Open" : "Close";

                            loggingServices.WriteLine<Adapter>("On/Off Event");
                            SpeechHelper.Speak(rtnMessage);
                        }
                    }
                };

            }
            catch(Exception e)
            {
                result = false;
            }

            


            return result;
        }
        #endregion
        
        #region ZigBeeTablesHandler
        private void ZigBeeSaveChanges(string command, object modelObject, out bool result)
        {
            result = false;
            int rtnCode = -1;
            try
            {
                using (var db = new Context())
                {
                    try
                    {
                        switch (command.ToUpper())
                        {
                            case "INSERT":
                                {
                                    if (modelObject is IOTOI.Model.ZigBee.ZigBeeEndDevice)
                                    {
                                        db.ZigBeeEndDevice.Add((IOTOI.Model.ZigBee.ZigBeeEndDevice)modelObject);
                                    }
                                    if (modelObject is IOTOI.Model.ZigBee.ZigBeeEndPoint)
                                    {
                                        db.ZigBeeEndPoint.Add((IOTOI.Model.ZigBee.ZigBeeEndPoint)modelObject);
                                    }
                                }
                                break;
                            case "UPDATE":
                                {
                                    if (modelObject is IOTOI.Model.ZigBee.ZigBeeEndDevice)
                                    {
                                        db.ZigBeeEndDevice.Update((IOTOI.Model.ZigBee.ZigBeeEndDevice)modelObject);
                                    }
                                    else if (modelObject is IOTOI.Model.ZigBee.ZigBeeEndPoint)
                                    {
                                        db.ZigBeeEndPoint.Update((IOTOI.Model.ZigBee.ZigBeeEndPoint)modelObject);
                                    }
                                    else if (modelObject is IOTOI.Model.ZigBee.ZigBeeInCluster)
                                    {
                                        db.ZigBeeInCluster.Update((IOTOI.Model.ZigBee.ZigBeeInCluster)modelObject);
                                    }
                                    else if (modelObject is IOTOI.Model.ZigBee.ZigBeeInClusterAttribute)
                                    {   
                                        db.ZigBeeInClusterAttribute.Update((IOTOI.Model.ZigBee.ZigBeeInClusterAttribute)modelObject);
                                    }
                                }
                                break;
                            case "DELETE":
                                {

                                }
                                break;
                            default:
                                {

                                }
                                break;
                        }
                        rtnCode = db.SaveChanges();
                        if (rtnCode > -1) result = true;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
        #endregion
    }
}

