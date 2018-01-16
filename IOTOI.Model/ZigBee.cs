using IOTOI.Model.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using System.Text;

namespace IOTOI.Model.ZigBee
{
    public class NotificationEntity : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class ZigBeeEndDevice : NotificationEntity
    {
        private ulong _macAddress;

        private ushort _networkAddress;
        private string _name;
        private bool _isConnected;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public ulong MacAddress
        {
            get { return _macAddress; }
            set { SetWithNotify(value, ref _macAddress); }
        }

        public ushort NetworkAddress
        {
            get { return _networkAddress; }
            set { SetWithNotify(value, ref _networkAddress); }
        }
        public string Name
        {
            get { return _name; }
            set { SetWithNotify(value, ref _name); }
        }
        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetWithNotify(value, ref _isConnected); }
        }
        public List<ZigBeeEndPoint> EndPoints { get; set; }
    }

    public class ZigBeeEndPoint : NotificationEntity
    {
        private int _id;

        private ushort _commanProfileId;

        private ushort _deviceId;
        private byte _epNum;

        private ulong _macAddress;

        private string _name;

        private string _customName;

        private int _protocolTypeId;


        private bool _isActivated;


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id
        {
            get { return _id; }
            set { SetWithNotify(value, ref _id); }
        }

        public ushort CommanProfileId
        {
            get { return _commanProfileId; }
            set { SetWithNotify(value, ref _commanProfileId); }
        }

        public ushort DeviceId
        {
            get { return _deviceId; }
            set { SetWithNotify(value, ref _deviceId); }
        }
        public byte EpNum
        {
            get { return _epNum; }
            set { SetWithNotify(value, ref _epNum); }
        }

        public ulong MacAddress
        {
            get { return _macAddress; }
            set { SetWithNotify(value, ref _macAddress); }
        }

        public string Name
        {
            get { return _name; }
            set { SetWithNotify(value, ref _name); }
        }

        public string CustomName
        {
            get { return _customName; }
            set { SetWithNotify(value, ref _customName); }
        }

        public int ProtocolTypeId
        {
            get { return _protocolTypeId; }
            set { SetWithNotify(value, ref _protocolTypeId); }
        }


        public bool IsActivated
        {
            get { return _isActivated; }
            set { SetWithNotify(value, ref _isActivated); }
        }

        public List<ZigBeeInCluster> ZigBeeInClusters { get; set; }

        public List<ZigBeeOutCluster> ZigBeeOutClusters { get; set; }

    }

    public class ZigBeeInCluster : NotificationEntity
    {
        private int _id;

        private int _parentId;

        private int _clusterId;

        private string _name;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id
        {
            get { return _id; }
            set { SetWithNotify(value, ref _id); }
        }

        public int ParentId
        {
            get { return _parentId; }
            set { SetWithNotify(value, ref _parentId); }
        }

        public int ClusterId
        {
            get { return _clusterId; }
            set { SetWithNotify(value, ref _clusterId); }
        }

        public string Name
        {
            get { return _name; }
            set { SetWithNotify(value, ref _name); }
        }

        public List<ZigBeeInClusterAttribute> ZigBeeInClusterAttributes { get; set; }

    }

    public class ZigBeeOutCluster : NotificationEntity
    {
        private int _id;

        private int _parentId;

        private int _clusterId;

        private string _name;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id
        {
            get { return _id; }
            set { SetWithNotify(value, ref _id); }
        }

        public int ParentId
        {
            get { return _parentId; }
            set { SetWithNotify(value, ref _parentId); }
        }

        public int ClusterId
        {
            get { return _clusterId; }
            set { SetWithNotify(value, ref _clusterId); }
        }

        public string Name
        {
            get { return _name; }
            set { SetWithNotify(value, ref _name); }
        }

        //public ZigBeeEndPoint EndPoint { get; set; }

        public List<ZigBeeOutClusterAttribute> ZigBeeOutClusterAttributes { get; set; }
    }

    public class ZigBeeInClusterAttribute : NotificationEntity
    {
        private int _id;

        private int _parentId;

        private string _name;

        private byte[] _attrValue;

        private byte _zigBeeType;


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id
        {
            get { return _id; }
            set { SetWithNotify(value, ref _id); }
        }

        public int ParentId
        {
            get { return _parentId; }
            set { SetWithNotify(value, ref _parentId); }
        }

        public string Name
        {
            get { return _name; }
            set { SetWithNotify(value, ref _name); }
        }

        public byte[] AttrValue
        {
            get { return _attrValue; }
            set { SetWithNotify(value, ref _attrValue); }
        }

        public byte ZigBeeType
        {
            get { return _zigBeeType; }
            set { SetWithNotify(value, ref _zigBeeType); }
        }

        public object RealValue
        {
            get
            {
                byte[] _attrValue = this.AttrValue;
                object value = null;
                ZigBeeHelper.GetValue(this.ZigBeeType, ref _attrValue, out value);
                return value;
            }
        }

        //public ZigBeeInCluster ZigBeeInCluster { get; set; }
    }

    public class ZigBeeOutClusterAttribute : NotificationEntity
    {

        private int _id;

        private int _parentId;

        private string _name;

        private byte[] _attrValue;

        private byte _zigBeeType;


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id
        {
            get { return _id; }
            set { SetWithNotify(value, ref _id); }
        }

        public int ParentId
        {
            get { return _parentId; }
            set { SetWithNotify(value, ref _parentId); }
        }

        public string Name
        {
            get { return _name; }
            set { SetWithNotify(value, ref _name); }
        }

        public byte[] AttrValue
        {
            get { return _attrValue; }
            set { SetWithNotify(value, ref _attrValue); }
        }

        public byte ZigBeeType
        {
            get { return _zigBeeType; }
            set { SetWithNotify(value, ref _zigBeeType); }
        }

        public object RealValue
        {
            get
            {
                byte[] _attrValue = this.AttrValue;
                object value = null;
                ZigBeeHelper.GetValue(this.ZigBeeType, ref _attrValue, out value);
                return value;
            }
        }

    }
}
