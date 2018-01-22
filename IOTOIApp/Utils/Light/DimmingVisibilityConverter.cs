using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace IOTOIApp.Utils.Light
{
    public class DimmingVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            //Dimming 기능이 있는 디바이스존재할시 사용
            //List<ZigBeeInCluster> zigBeeInClusters = value as List<ZigBeeInCluster>;

            //foreach (ZigBeeInCluster zigBeeInCluster in zigBeeInClusters.Where(z => z.ClusterId == 6))
            //{
            //    foreach (ZigBeeInClusterAttribute zigBeeInClusterAttribute in zigBeeInCluster.ZigBeeInClusterAttributes)
            //    {
            //        return (bool)zigBeeInClusterAttribute.RealValue ? 1 : 0;
            //    }

            //}
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
