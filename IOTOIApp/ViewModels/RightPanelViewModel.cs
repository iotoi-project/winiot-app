using System;

using GalaSoft.MvvmLight;
using Windows.Devices.WiFi;
using System.Diagnostics;
using IOTOIApp.Utils;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Core;
//using AdapterLib;
using System.Globalization;
using IOTOIApp.Services;
using Windows.Foundation.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IOTOIApp.Models;

namespace IOTOIApp.ViewModels
{
    public class RightPanelViewModel : ViewModelBase
    {
        private string _currentTime;
        public string CurrentTime
        {
            get { return _currentTime; }
            set { Set(ref _currentTime, value); }
        }

        private string _meridiem;
        public string Meridiem
        {
            get { return _meridiem; }
            set { Set(ref _meridiem, value); }
        }
        
        private string _currentDate;
        public string CurrentDate
        {
            get { return _currentDate; }
            set { Set(ref _currentDate, value); }
        }

        private string _tempMax;
        public string TempMax
        {
            get { return _tempMax; }
            set { Set(ref _tempMax, value); }
        }

        private string _tempMin;
        public string TempMin
        {
            get { return _tempMin; }
            set { Set(ref _tempMin, value); }
        }

        private string _weatherIcon;
        public string WeatherIcon
        {
            get { return _weatherIcon; }
            set { Set(ref _weatherIcon, value); }
        }

        private DispatcherTimer DateTimetimer;
        private DispatcherTimer Weathertimer;

        private ObservableCollection<Weather> _weatherListSources = new ObservableCollection<Weather>();
        public ObservableCollection<Weather> WeatherListSources
        {
            get { return _weatherListSources; }
            set { Set(ref _weatherListSources, value); }
        }

        public RightPanelViewModel()
        {
            DateTimetimer = new DispatcherTimer();
            DateTimetimer.Tick += DateTimetimer_Tick;
            DateTimetimer.Interval = TimeSpan.FromSeconds(10);
            DateTimetimer.Start();

            Weathertimer = new DispatcherTimer();
            Weathertimer.Tick += Weather_Tick;
            Weathertimer.Interval = TimeSpan.FromHours(3);
            Weathertimer.Start();

            GetCurrentDateTime();
            GetWeather();
        }
                
        private void DateTimetimer_Tick(object sender, object e)
        {
            GetCurrentDateTime();
        }

        private void Weather_Tick(object sender, object e)
        {
            GetWeather();
        }

        private void GetCurrentDateTime()
        {
            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);

            DateTime t = localTime.ToDateTime();

            CurrentTime = t.ToString("hh:mm");
            Meridiem = t.ToString("tt");
            CurrentDate = t.ToString("MMMM . dd . yyyy");
        }

        private async void GetWeather()
        {
            var position = await LocationService.GetPosition();

            double Latitude = (position == null) ? 37.5683 : position.Coordinate.Point.Position.Latitude;
            double Longitude = (position == null) ? 126.9778 : position.Coordinate.Point.Position.Longitude;
            Debug.WriteLine("Latitude[" + Latitude + "] Longitude[" + Longitude + "]");
            try
            {
                ApixuWeatherObject myWeather = await ApixuWeatherService.GetWeather(Latitude, Longitude);

                if(myWeather != null)
                {
                    WeatherListSources.Clear();
                    bool IsToday = true;
                    foreach (Forecastday forecastDay in myWeather.forecast.forecastday)
                    {
                        string DayOfWeek = DateTime.ParseExact(forecastDay.date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture).ToString("ddd");
                        WeatherListSources.Add(new Weather
                        {
                            IsToday = IsToday,
                            DayOfWeek = DayOfWeek,
                            WeatherIcon = ApixuWeatherService.ConditionCodeToIcon(forecastDay.day.condition.code),
                            Temp = string.Format("{0}¡Æ", Math.Round(forecastDay.day.avgtemp_c).ToString())
                        });
                        IsToday = false;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("GetWeather error " + e.Message);
            }
        }
        
        //private async void GetWeather()
        //{
        //    var position = await LocationService.GetPosition();

        //    double Latitude = (position == null) ? 37.5683 : position.Coordinate.Latitude;
        //    double Longitude = (position == null) ? 126.9778 : position.Coordinate.Longitude;

        //    Debug.WriteLine("Latitude[" + Latitude + "] Longitude[" + Longitude + "]");
        //    try
        //    {
        //        RootObject myWeather = await OpenWeatherMapProxy.GetWeather(Latitude, Longitude);

        //        if (myWeather != null)
        //        {
        //            TempMax = String.Format("{0:F1}", myWeather.main.temp_max);
        //            TempMin = String.Format("{0:F1}", myWeather.main.temp_min);

        //            WeatherIconMapping();
        //            string weatherIcon = "\xF00D";
        //            WeatherIconMap.TryGetValue(myWeather.weather[0].icon, out weatherIcon);
        //            WeatherIcon = weatherIcon;
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //private StringMap WeatherIconMap = new StringMap();
        //private void WeatherIconMapping()
        //{
        //    WeatherIconMap.Add("01d", "\xF00D"); //clear sky
        //    WeatherIconMap.Add("01n", "\xF02E"); //clear sky

        //    WeatherIconMap.Add("02d", "\xF002"); //few clouds
        //    WeatherIconMap.Add("02n", "\xF086"); //few clouds

        //    WeatherIconMap.Add("03d", "\xF013"); //scattered clouds
        //    WeatherIconMap.Add("03n", "\xF013"); //scattered clouds

        //    WeatherIconMap.Add("04d", "\xF014"); //broken clouds
        //    WeatherIconMap.Add("04n", "\xF014"); //broken clouds

        //    WeatherIconMap.Add("09d", "\xF019"); //shower rain
        //    WeatherIconMap.Add("09n", "\xF019"); //shower rain

        //    WeatherIconMap.Add("10d", "\xF008"); //rain
        //    WeatherIconMap.Add("10n", "\xF028"); //rain

        //    WeatherIconMap.Add("11d", "\xF010"); //thunderstorm
        //    WeatherIconMap.Add("11n", "\xF010"); //thunderstorm

        //    WeatherIconMap.Add("13d", "\xF01B"); //snow
        //    WeatherIconMap.Add("13n", "\xF01B"); //snow

        //    WeatherIconMap.Add("50d", "\xF021"); //mist
        //    WeatherIconMap.Add("50n", "\xF021"); //mist 
        //}
    }
}
