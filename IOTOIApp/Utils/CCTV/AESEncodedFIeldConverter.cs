using IOTOI.Model.Utils;
using System;
using Windows.UI.Xaml.Data;

namespace IOTOIApp.Utils.CCTV
{
    public class AESEncodedFIeldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (string.IsNullOrEmpty((string)value))
                return "";
            else
            {
                return AESCipher.AES_Decrypt((string)value);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
            //throw new NotImplementedException();
        }
    }
}
