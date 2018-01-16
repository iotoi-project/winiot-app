using IOTOI.Model.ZigBee;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace IOTOIApp.Utils.Plug
{
    public class PlugListViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SiglePlugTemplate { get; set; }

        public DataTemplate MultiplePlugTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            ZigBeeEndDevice endDevice = item as ZigBeeEndDevice;

            if (endDevice.EndPoints.Count > 1)
            {
                return MultiplePlugTemplate;
            }
            else
            {
                return SiglePlugTemplate;
            }
        }
    }
}
