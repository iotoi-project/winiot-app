using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace IOTOIApp.Utils.Light
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Visibility.Collapsed;
            else
            {
                switch (parameter)
                {
                    case "On": return (bool)value ? Visibility.Visible : Visibility.Collapsed;
                    case "Off": return !(bool)value ? Visibility.Visible : Visibility.Collapsed;
                    default: return Visibility.Collapsed;

                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
