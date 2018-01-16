using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZWaveAdapterLib
{
    public sealed class SerialPortInfo
    {
        internal SerialPortInfo(string id, string name)
        {
            PortID = id;
            Name = name;
        }
        public string PortID { get; }
        public string Name { get; }

        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                if (value)
                {
                    Watcher.Instance.AddController(PortID, Name);
                }
                else
                {
                    Watcher.Instance.RemoveController(PortID);
                }
            }
        }
    }
}
