using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace IOTOIApp.Utils
{
    public  class ConverHexToColor
    {
        public static SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = 0xFF;
            byte r = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }
    }
}
