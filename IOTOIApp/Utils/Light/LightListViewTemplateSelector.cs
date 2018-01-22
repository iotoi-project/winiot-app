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

namespace IOTOIApp.Utils.Light
{
    public class LightListViewTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SigleLightTemplate { get; set; }

        public DataTemplate MultipleLightTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            ZigBeeEndDevice endDevice = item as ZigBeeEndDevice;

            if (endDevice.EndPoints.Count > 1)
            {
                return MultipleLightTemplate;
            }
            else
            {
                return SigleLightTemplate;
            }
        }
    }
}
