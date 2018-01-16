using IOTOI.Model.ZigBee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace IOTOIApp.Utils.Plug
{
    public class PlugToggleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            List<ZigBeeInCluster> zigBeeInClusters = value as List<ZigBeeInCluster>;

            foreach (ZigBeeInCluster zigBeeInCluster in zigBeeInClusters.Where(z => z.ClusterId == 6))
            {
                foreach (ZigBeeInClusterAttribute zigBeeInClusterAttribute in zigBeeInCluster.ZigBeeInClusterAttributes)
                {
                    switch (parameter)
                    {
                        case "On": return (bool)zigBeeInClusterAttribute.RealValue ? Visibility.Visible : Visibility.Collapsed;
                        case "Off": return !(bool)zigBeeInClusterAttribute.RealValue ? Visibility.Visible : Visibility.Collapsed;
                        default: return Visibility.Collapsed;
                    }
                }

                }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
