using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTOIApp.Models
{
    public class Weather
    {
        public bool IsToday { get; set; }
        public string DayOfWeek { get; set; }
        public string WeatherIcon { get; set; }
        public string Temp { get; set; }
    }
}
