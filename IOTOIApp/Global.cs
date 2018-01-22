using IOTOIApp.Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IOTOIApp
{
    public class Global
    {
        public ThermostatAPI ThermostatAPI { get; private set; }

        public string ProductID { get; private set; }
        public string ProductSecret { get; private set; }
        public string AuthorizationURL { get; private set; }
        public string AuthorizationEndpoint { get; private set; }

        public string AppParams { get; set; }

        public Timer TemperatureCheckTimer = null;


        #region Singleton

        public static Global Instance { get; private set; }


        /// <summary>
        /// Singleton 생성자
        /// </summary>
        static Global()
        {
            Instance = new Global();
        }

        #endregion Singleton


        /// <summary>
        /// 생성자
        /// </summary>
        private Global()
        {
            ThermostatAPI = new ThermostatAPI();

            AuthorizationEndpoint = "https://api.home.nest.com/oauth2/access_token";

            //AppParams = "fan_timer_active 30";
        }


        public void SetAuthValue(string id, string secret, string url)
        {
            ProductID = id;
            ProductSecret = secret;
            AuthorizationURL = url;
        }


        //
        // WPF Sample Binding
        //
        // ItemsSource="{Binding Source={x:static local:Global.Instance}, Path=ItemCollection}"
        //
    }
}
