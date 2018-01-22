// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.UI.Xaml.Data;

namespace IOTOIApp.Utils
{
    public class WifiGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is byte))
            {
                return null;
            }

            var strength = (byte)value;

            switch (strength)
            {
                case 0:
                case 1:
                    return "\xE872";
                case 2:
                    return "\xE873";
                case 3:
                    return "\xE874";
                case 4:
                    return "\xE701";
                default:
                    return "\xE701";
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
