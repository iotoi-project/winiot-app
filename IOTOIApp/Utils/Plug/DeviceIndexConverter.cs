using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace IOTOIApp.Utils.Plug
{
    public class DeviceIndexConverter : DependencyObject, IValueConverter
    {
        public ListView DeivceListView
        {
            get { return (ListView)GetValue(DeivceListViewProperty); }
            set { SetValue(DeivceListViewProperty, value); }

        }

        public static readonly DependencyProperty DeivceListViewProperty =
            DependencyProperty.Register("DeivceListView",
                                    typeof(ListView),
                                    typeof(DeviceIndexConverter),
                                    new PropertyMetadata(null));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            try
            {
                ListViewItem DeivceItem = DeivceListView.ContainerFromItem(value) as ListViewItem;
                int index = DeivceListView.IndexFromContainer(DeivceItem);
                return index + 1;
            }
            catch(Exception e)
            {
                Debug.WriteLine("DeviceIndexConverter Exception" + e.Message);
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

