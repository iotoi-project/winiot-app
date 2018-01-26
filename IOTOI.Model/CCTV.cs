using IOTOI.Model.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOTOI.Model
{
    public class CCTV
    {
        public int CCTVId { get; set; }
        public string IpAddress { get; set; }
        public string AccountId { get; set; }

        string _accountPass;
        public string AccountPass
        {
            get { return _accountPass; }
            set
            {
                if(value != null && value.Length > 0) _accountPass = AESCipher.AES_Encrypt(value);                
            }
        }

        public string CCTVName { get; set; }
        public string CCTVType { get; set; }

        public string CgiUrl
        {
            get
            {
                switch (CCTVType.ToUpper())
                {
                    case "FOSCAM":
                        return String.Format("/cgi-bin/CGIProxy.fcgi?cmd=snapPicture2&usr={0}&pwd={1}", AccountId, AESCipher.AES_Decrypt(AccountPass));
                    case "SUNELL":
                        return String.Format("/cgi-bin/image.cgi?userName={0}&password={1}&cameraID=1&quality=1", AccountId, AESCipher.AES_Decrypt(AccountPass));
                }
                return "";
            }
        }
    }
}
