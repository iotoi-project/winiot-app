using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Headers;

namespace IOTOI.Common.Nest
{
    public class ThermostatAPI
    {
        DeviceParse Parse { get; set; }

        public bool AuthorizationError { get; private set; }
        public string ErrorMessage { get; private set; }

        public List<ThermostatDevice> ThermostatDevices { get; private set; }
        public List<Structure> Structures { get; private set; }

        //string _accessToken = "c.QQcpHZzsnoB9WZ5CmjWAUhrkn9JPQoMZZFiJE2zNon9RNGJMI0Y7goGiGdYlQKMhGESbolXTAJs5vpUofTTChTTuSi3dWNytsPbPNJSXcT9CyLBdHZXv7p9QkBlPugrT7fDIndtwpVsaoPWf";
        string _accessToken;
        public string AuthToken
        {
            get { return _accessToken; }
            private set { _accessToken = value; }
        }


        HttpClient ApiClient()
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders
                  .Accept
                  .Add(new HttpMediaTypeWithQualityHeaderValue("application/json"));    //ACCEPT header

            return client;
        }

        // https://stackoverflow.com/questions/31150900/httprequestexception-when-trying-putasync        
        System.Net.Http.HttpClient HttpApiClient()
        {
            System.Net.Http.HttpClientHandler clientHandler = new System.Net.Http.HttpClientHandler();
            clientHandler.AllowAutoRedirect = false;

            return new System.Net.Http.HttpClient(clientHandler);
        }


        public ThermostatAPI()
        {
            Parse = new DeviceParse();
            AuthorizationError = false;
        }


        public void ApplyAccessToken(string token)
        {
            AuthToken = token;
        }


        public async Task<bool> GetDevices()
        {
            AuthorizationError = false;

            using (HttpClient client = ApiClient())
            {
                try
                {
                    string api = string.Format("https://developer-api.nest.com/?auth={0}", AuthToken);

                    HttpResponseMessage response = await client.GetAsync(new Uri(api));
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent.Contains("error"))
                    {
                        if (responseContent.Contains("authorization code expired") || responseContent.Contains("authorization code not found"))
                        {
                            AuthorizationError = true;
                        }

                        return false;
                    }

                    if (Parse.Parse(responseContent))
                    {
                        ThermostatDevices = Parse.ThermostatDevices;
                        Structures = Parse.Structures;

                        return true;
                    }
                    else
                    {
                        ThermostatDevices = new List<ThermostatDevice>();
                        Structures = new List<Structure>();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }

                return false;
            }
        }


        // 목표 온도 설정
        // 9 ~ 32 (℉ = ℃ * 1.8000 + 32.00)
        public async Task<string> SetTemperature(ThermostatDevice device, double target, string scale = "C")
        {
            AuthorizationError = false;

            string tsc = ("C" == scale) ? "target_temperature_c" : "target_temperature_f";

            string api = string.Format("https://developer-api.nest.com/devices/thermostats/{0}?auth={1}",
                                       device.device_id, AuthToken);

            string body = "{\"" + tsc + "\": " + target.ToString() + ", \"temperature_scale\": \"" + scale + "\"}";

            using (System.Net.Http.HttpClient client = HttpApiClient())
            {
                try
                {
                    System.Net.Http.HttpResponseMessage response = await client.PutAsync(new Uri(api), new System.Net.Http.StringContent(body));

                    Uri url = response.Headers.Location;
                    response = await client.PutAsync(url, new System.Net.Http.StringContent(body));

                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent.Contains("error"))
                    {
                        if (responseContent.Contains("authorization code expired") || responseContent.Contains("authorization code not found"))
                        {
                            AuthorizationError = true;
                        }

                        ErrorMessage = Parse.ErrorMessage(responseContent);
                        return ErrorMessage;
                    }

                    return responseContent;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }

                return ErrorMessage;
            }
        }


        // 현재 온도 표시
        public async Task<string> GetTemperature(ThermostatDevice device, string scale = "C")
        {
            AuthorizationError = false;

            string tsc = ("C" == scale) ? "ambient_temperature_c" : "ambient_temperature_f";

            string api = string.Format("https://developer-api.nest.com/devices/thermostats/{0}/{1}?auth={2}",
                                       device.device_id, tsc, AuthToken);

            using (HttpClient client = ApiClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(new Uri(api));
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent.Contains("error"))
                    {
                        if (responseContent.Contains("authorization code expired") || responseContent.Contains("authorization code not found"))
                        {
                            AuthorizationError = true;
                        }

                        ErrorMessage = Parse.ErrorMessage(responseContent);
                        return ErrorMessage;
                    }

                    return responseContent;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }

                return ErrorMessage;
            }
        }


        // 팬 구동 시간 설정
        // https://developers.nest.com/documentation/cloud/api-thermostat#fan_timer_duration
        // Values : 15, 30, 45, 60, 120, 240, 480, 720
        public async Task<string> SetFanTimer(ThermostatDevice device, int minute)
        {
            AuthorizationError = false;

            string api = string.Format("https://developer-api.nest.com/devices/thermostats/{0}?auth={1}",
                                       device.device_id, AuthToken);

            string body = "{\"fan_timer_active\": true, \"fan_timer_duration\": " + minute.ToString() + "}";

            using (System.Net.Http.HttpClient client = HttpApiClient())
            {
                try
                {
                    System.Net.Http.HttpResponseMessage response = await client.PutAsync(new Uri(api), new System.Net.Http.StringContent(body));

                    Uri url = response.Headers.Location;
                    response = await client.PutAsync(url, new System.Net.Http.StringContent(body));

                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent.Contains("error"))
                    {
                        if (responseContent.Contains("authorization code expired") || responseContent.Contains("authorization code not found"))
                        {
                            AuthorizationError = true;
                        }

                        ErrorMessage = Parse.ErrorMessage(responseContent);
                        return ErrorMessage;
                    }

                    return responseContent;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }

                return ErrorMessage;
            }
        }


        public async Task<string> SetFanOff(ThermostatDevice device)
        {
            AuthorizationError = false;

            string api = string.Format("https://developer-api.nest.com/devices/thermostats/{0}?auth={1}",
                                       device.device_id, AuthToken);

            string body = "{\"fan_timer_active\": false}";

            using (System.Net.Http.HttpClient client = HttpApiClient())
            {
                try
                {
                    System.Net.Http.HttpResponseMessage response = await client.PutAsync(new Uri(api), new System.Net.Http.StringContent(body));

                    Uri url = response.Headers.Location;
                    response = await client.PutAsync(url, new System.Net.Http.StringContent(body));

                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent.Contains("error"))
                    {
                        if (responseContent.Contains("authorization code expired") || responseContent.Contains("authorization code not found"))
                        {
                            AuthorizationError = true;
                        }

                        ErrorMessage = Parse.ErrorMessage(responseContent);
                        return ErrorMessage;
                    }

                    return responseContent;
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                }

                return ErrorMessage;
            }
        }
    }
}
