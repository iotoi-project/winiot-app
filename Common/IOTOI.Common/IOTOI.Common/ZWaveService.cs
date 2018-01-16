using MetroLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZWaveAdapterLib;

namespace IOTOI.Common
{
    public static class ZWaveService
    {
        public static Watcher Watcher { get; set; }

        private static int IsZWaveStatus = -1;

        static IOTOI.Model.LoggingServices.LoggingServices loggingServices = new Model.LoggingServices.LoggingServices();
        static ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(ZWaveService));

        #region Init ZWave
        public static int InitZWave()
        {
            if (!ApplicationState.Instance.SerialPorts.Any())
            {
                //InitTable();
                Watcher = Watcher.Instance ?? new Watcher(Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher);
                IsZWaveStatus = Watcher.Initialize();
                ApplicationState.Instance.InitializeAsync().ContinueWith((t) =>
                {
                    GetSerialPorts();

                    //IEnumerable<Node> Nodes = Watcher.Nodes;
                    //Debug.WriteLine("1234");
                }).ContinueWith((t) => {
                    //IEnumerable<Node> Nodes = Watcher.Nodes;
                    //Debug.WriteLine("1234");
                });
            }
            else if (ApplicationState.Instance.SerialPorts.Count == 1)
            {
                //InitTable();
            }
            return IsZWaveStatus;
        }
        #endregion

        private static void GetSerialPorts()
        {
            if (!ApplicationState.Instance.SerialPorts.Any())
            {
                //var _ = new Windows.UI.Popups.MessageDialog("No serial ports found").ShowAsync();
                //Debug.WriteLine("No serial ports found");
                loggingServices.Write(Log, "No serial ports found", LogLevel.Info);
            }
            else if (ApplicationState.Instance.SerialPorts.Count == 1)
            {

                ApplicationState.Instance.SerialPorts[0].IsActive = true; //Assume if there's only one port, that's the ZStick port


                //IEnumerable<Node> Nodes = Watcher.Nodes;
                //Debug.WriteLine("1234");
            }
        }
    }
}
