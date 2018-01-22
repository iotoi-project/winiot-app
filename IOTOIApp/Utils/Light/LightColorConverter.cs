using IOTOI.Model.ZigBee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace IOTOIApp.Utils.Light
{
    public class LightColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            List<ZigBeeInCluster> zigBeeInClusters = value as List<ZigBeeInCluster>;

            foreach (ZigBeeInCluster zigBeeInCluster in zigBeeInClusters.Where(z => z.ClusterId == 6))
            {
                foreach (ZigBeeInClusterAttribute zigBeeInClusterAttribute in zigBeeInCluster.ZigBeeInClusterAttributes)
                {
                    return (bool)zigBeeInClusterAttribute.RealValue ? new SolidColorBrush(Windows.UI.Colors.Orange) : new SolidColorBrush(Windows.UI.Colors.DimGray);
                }

            }
            return new SolidColorBrush(Windows.UI.Colors.DimGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
