using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace IOTOIApp.Utils.CCTV
{
    public class TextBoxBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            SolidColorBrush DefaultColorBrush = (SolidColorBrush)Application.Current.Resources["SystemControlForegroundBaseMediumLowBrush"];
            SolidColorBrush ErrorColorBrush = new SolidColorBrush(Windows.UI.Colors.Red);

            if (value == null)
                //return new SolidColorBrush(Windows.UI.Colors.Red);
                return DefaultColorBrush;
            else
            {
                return ((string)parameter == (string)value) ? ErrorColorBrush : DefaultColorBrush;

                //switch (parameter)
                //{
                //    case "IpAddress": return (bool)value ? ErrorColorBrush : DefaultColorBrush;
                //    case "CCTVName": return (bool)value ? ErrorColorBrush : DefaultColorBrush;

                //    default: return DefaultColorBrush;
                //}
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ErrorTextVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            if (value == null)
                return Visibility.Collapsed;
            else
            {
                return string.IsNullOrEmpty((string)value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
