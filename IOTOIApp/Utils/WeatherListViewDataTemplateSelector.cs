using IOTOIApp.Models;
using IOTOIApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace IOTOIApp.Utils
{
    [ContentProperty(Name = "Items")]
    class WeatherListViewDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TodayWeatherTemplate { get; set; }

        public DataTemplate WeekWeatherTemplate { get; set; }

        public Dictionary<string, DataTemplate> Items { get; set; }
        public static Dictionary<string, DataTemplate> DataTemplates { get; set; }

        public WeatherListViewDataTemplateSelector()
        {
            Items = new Dictionary<string, DataTemplate>();
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var Today = item as Weather;
            DataTemplate WeatherTemplate;
            if (Today.IsToday)
            {
                Items.TryGetValue("TodayWeatherTemplate", out WeatherTemplate);
                return WeatherTemplate;
            }
            else
            {
                Items.TryGetValue("WeekWeatherTemplate", out WeatherTemplate);
                return WeatherTemplate;
            }
        }

    }
}
