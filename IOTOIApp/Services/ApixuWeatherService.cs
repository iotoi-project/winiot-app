using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace IOTOIApp.Services
{
    class ApixuWeatherService
    {
        static string APPID = "ad7c54459f4640999ae74849172510";
        static short Days = 6;

        static StringMap WeatherIconMap = new StringMap();
        static ApixuWeatherService()
        {
            WeatherIconMap.Add("1000", "\xF00D");
            WeatherIconMap.Add("1003", "\xF002");
            WeatherIconMap.Add("1006", "\xF013");
            WeatherIconMap.Add("1009", "\xF013");
            WeatherIconMap.Add("1030", "\xF014");
            WeatherIconMap.Add("1063", "\xF008");
            WeatherIconMap.Add("1066", "\xF00A");
            WeatherIconMap.Add("1069", "\xF004");
            WeatherIconMap.Add("1072", "\xF019");
            WeatherIconMap.Add("1087", "\xF005");
            WeatherIconMap.Add("1114", "\xF01B");
            WeatherIconMap.Add("1117", "\xF01B");
            WeatherIconMap.Add("1135", "\xF014");
            WeatherIconMap.Add("1147", "\xF014");
            WeatherIconMap.Add("1150", "\xF019");
            WeatherIconMap.Add("1153", "\xF019");
            WeatherIconMap.Add("1168", "\xF019");
            WeatherIconMap.Add("1171", "\xF019");
            WeatherIconMap.Add("1180", "\xF008");
            WeatherIconMap.Add("1183", "\xF019");
            WeatherIconMap.Add("1186", "\xF008");
            WeatherIconMap.Add("1189", "\xF019");
            WeatherIconMap.Add("1192", "\xF008");
            WeatherIconMap.Add("1195", "\xF019");
            WeatherIconMap.Add("1198", "\xF019");
            WeatherIconMap.Add("1201", "\xF019");
            WeatherIconMap.Add("1204", "\xF015");
            WeatherIconMap.Add("1207", "\xF015");
            WeatherIconMap.Add("1210", "\xF00A");
            WeatherIconMap.Add("1213", "\xF01B");
            WeatherIconMap.Add("1216", "\xF00A");
            WeatherIconMap.Add("1219", "\xF01B");
            WeatherIconMap.Add("1222", "\xF00A");
            WeatherIconMap.Add("1225", "\xF01B");
            WeatherIconMap.Add("1237", "\xF017");
            WeatherIconMap.Add("1240", "\xF008");
            WeatherIconMap.Add("1243", "\xF008");
            WeatherIconMap.Add("1246", "\xF008");
            WeatherIconMap.Add("1249", "\xF004");
            WeatherIconMap.Add("1252", "\xF004");
            WeatherIconMap.Add("1255", "\xF00A");
            WeatherIconMap.Add("1258", "\xF00A");
            WeatherIconMap.Add("1261", "\xF009");
            WeatherIconMap.Add("1264", "\xF009");
            WeatherIconMap.Add("1273", "\xF005");
            WeatherIconMap.Add("1276", "\xF01D");
            WeatherIconMap.Add("1279", "\xF00E");
            WeatherIconMap.Add("1282", "\xF01D");
        }
        public async static Task<ApixuWeatherObject> GetWeather(double lat, double lon)
        {
            try
            {
                string url = String.Format("http://api.apixu.com/v1/forecast.json?days={0}&q={1},{2}&key={3}", Days, lat, lon, APPID);
                var http = new HttpClient();
                var response = await http.GetAsync(url);
                var result = await response.Content.ReadAsStringAsync();
                var serializer = new DataContractJsonSerializer(typeof(ApixuWeatherObject));

                var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
                return (ApixuWeatherObject)serializer.ReadObject(ms);
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("ApixuWeatherService GetWeather error : " + e);
                return null;
            }

        }

        public static string ConditionCodeToIcon(int code)
        {
            string WeatherIcon = "\xF00D";
            WeatherIconMap.TryGetValue(code.ToString(), out WeatherIcon);
            return WeatherIcon;
        }
    }


    [DataContract]
    public class Condition
    {
        [DataMember]
        public int code { get; set; }
    }

    [DataContract]
    public class Day
    {
        [DataMember]
        public decimal avgtemp_c { get; set; }

        [DataMember]
        public Condition condition { get; set; }
    }

    [DataContract]
    public class Forecastday
    {
        [DataMember]
        public string date { get; set; }

        [DataMember]
        public Day day { get; set; }

    }

    [DataContract]
    public class Forecast
    {
        [DataMember]
        public List<Forecastday> forecastday { get; set; }

    }

    [DataContract]
    public class ApixuWeatherObject
    {
        [DataMember]
        public Forecast forecast { get; set; }
    }
}
