using IOTOI.Model.ZigBee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace IOTOIApp.Utils.Plug
{
    public class PlugColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            List<ZigBeeInCluster> zigBeeInClusters = value as List<ZigBeeInCluster>;

            foreach (ZigBeeInCluster zigBeeInCluster in zigBeeInClusters.Where(z => z.ClusterId == 6))
            {
                foreach (ZigBeeInClusterAttribute zigBeeInClusterAttribute in zigBeeInCluster.ZigBeeInClusterAttributes)
                {
                    return (bool)zigBeeInClusterAttribute.RealValue ? 1 : 0.3;
                }

            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
