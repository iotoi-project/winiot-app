using System;

using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using Windows.UI.Xaml.Controls;
//using AdapterLib;

using System.Diagnostics;
using Windows.System.Threading;
using Windows.Foundation;
using System.Threading.Tasks;
using System.Collections.Generic;
//using static AdapterLib.XBeeAction;
//using IOTOIApp.Model;

namespace IOTOIApp.ViewModels
{
    public class CortanaViewModel : ViewModelBase
    {
        private string _deviceStatus;

        public string DeviceStatus
        {
            get { return _deviceStatus; }
            set { Set(ref _deviceStatus, value); }
        }

        public ICommand CheckCommand { get; private set; }

        public ICommand Test1Command { get; private set; }

        //private static Adapter m_zigBeeAdapter = Utils.Xbee.getAdapter;


        private static UInt64 TEST_MAC_ADDRESS = 0x000D6F000C13EA48;
        private static byte TEST_END_POINT = 0x01;

      

        public CortanaViewModel()
        {
            //Test1Command = new RelayCommand<ItemClickEventArgs>(Test1);
            _deviceStatus = "FALSE";

            //IList<EndDevice> EndDevices = XBeeAction.DiscoverEndDevices();
        }
        

        //private async void Test1(ItemClickEventArgs args)
        //{
        //    //await ThreadPool.RunAsync(new WorkItemHandler((IAsyncAction action) =>
        //    //{
        //    //    try
        //    //    {
        //    //        Debug.WriteLine("Btn Clicked!");
        //    //        XBeeAction.PowerOff(TEST_MAC_ADDRESS, TEST_END_POINT);
        //    //    }
        //    //    catch
        //    //    {
        //    //        throw;
        //    //    }
        //    //}));

        //    //await testRun();
        //    /*
        //    if (m_zigBeeAdapter.DeviceList.TryGetValue(TEST_MAC_ADDRESS, out m_testedDevice) &&
        //       m_testedDevice.EndPointList.TryGetValue(TEST_END_POINT, out m_testedEndPoint))
        //    {
        //        Debug.WriteLine("End point: {0} - {1} - Mac address: 0x{2:X}, Id: 0x{3:X2}, ZigBee profile 0x{4:X4}",
        //            m_testedEndPoint.Vendor, m_testedEndPoint.Model, m_testedDevice.MacAddress, m_testedEndPoint.Id, m_testedEndPoint.CommandProfileId);
        //        _deviceStatus = "TRUE";
        //        onOffcluster = m_testedEndPoint.GetCluster(OnOffCluster.CLUSTER_ID);
        //        if (onOffcluster != null)
        //        {
        //            onOffcluster.CommandList.TryGetValue(OnOffCluster.COMMAND_OFF, out commandOff);
        //            onOffcluster.CommandList.TryGetValue(OnOffCluster.COMMAND_TOGGLE, out commandToggle);
        //        }

        //        SendOnOffCommand(commandOff);

        //    }
        //    */
        //    #region ¿¬µ¿
        //    //ZclCommand commandToggle = null;
        //    //ZclCommand commandOff = null;

        //    // look for OnOff cluster in the In cluster list of this end point
        //    //onOffcluster = m_testedEndPoint.GetCluster(OnOffCluster.CLUSTER_ID);
        //    /*if (onOffcluster != null)
        //    {
        //        onOffcluster.CommandList.TryGetValue(OnOffCluster.COMMAND_OFF, out commandOff);
        //        onOffcluster.CommandList.TryGetValue(OnOffCluster.COMMAND_TOGGLE, out commandToggle);
        //    }*/

        //    // set light on
        //    //SendOnOffCommand(commandOff);

        //    /*
        //    for (int index = 0; index < 9; index++)
        //    {
        //        // toggle light
        //        SendOnOffCommand(commandToggle);
        //    }

        //    // set light on
        //    SendOnOffCommand(commandOn);
        //    */

        //    #endregion



        //}
        
    }
}
