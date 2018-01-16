using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace ZWaveAdapterLib
{
    public class ApplicationState
    {
        static ApplicationState _state;
        public static ApplicationState Instance
        {
            get
            {
                return _state ?? (_state = new ApplicationState());
            }
        }

        Task initializeTask;
        private ApplicationState()
        {
        }
        public Task InitializeAsync()
        {
            initializeTask = Initialize();
            return initializeTask;
        }

        private async Task Initialize()
        {
            var serialPortSelector = Windows.Devices.SerialCommunication.SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(serialPortSelector);
            foreach (var item in devices)
            {
                SerialPorts.Add(new SerialPortInfo(item.Id, item.Name));
                Debug.WriteLine("SerialPorts Added Id = [" + item.Id + "], name = [" + item.Name + "]");
            }
        }

        public ObservableCollection<SerialPortInfo> SerialPorts { get; } = new ObservableCollection<SerialPortInfo>();

    }
}
