using IOTOI.Model.ZigBee;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace IOTOIApp.Utils
{
    public class IsOpenToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ZigBeeEndPoint endPoint =  value as ZigBeeEndPoint;

            if (value == null)
            {
                return "Unknown";
            }
            else if(!endPoint.IsActivated)
            {
                return "Inactive";
            }
            else
            {
                foreach (ZigBeeInCluster zigBeeInCluster in endPoint.ZigBeeInClusters.Where(z => z.ClusterId == 6))
                {
                    foreach (ZigBeeInClusterAttribute zigBeeInClusterAttribute in zigBeeInCluster.ZigBeeInClusterAttributes)
                    {
                        if((string)parameter == "Plug")
                        {
                            return (bool)zigBeeInClusterAttribute.RealValue ? " On" : " Off";
                        }
                        else
                        {
                            return (bool)zigBeeInClusterAttribute.RealValue ? " On" : " Off";
                        }
                        
                    }

                }
                return "Unknown";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class EndDeviceStatusPanelVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (value == null)
                return Visibility.Collapsed;
            else
            {
                ushort EndDeviceId = (ushort)value;
                switch (parameter)
                {
                    case "Lamp": return (EndDeviceId == 256) ? Visibility.Visible : Visibility.Collapsed;
                    case "Plug": return (EndDeviceId == 81) ? Visibility.Visible : Visibility.Collapsed;
                    default: return (EndDeviceId == 0 || EndDeviceId == 1026) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class IsConnectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (value == null)
                return "Unknown";
            else
                return (bool)value ? "Connected" : "Disconnected";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
