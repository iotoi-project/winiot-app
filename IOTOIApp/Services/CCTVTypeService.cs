using IOTOI.Model.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace IOTOIApp.Services
{
    public class CCTVTypeService
    {
        static Dictionary<string, string> IPCameraUris = new Dictionary<string, string>
        {
            {"Sunell", "cgi-bin/param.cgi?userName={0}&password={1}&action=get&type=deviceInfo"},
            {"Foscam", "cgi-bin/CGIProxy.fcgi?usr={0}&pwd={1}&cmd=getIPInfo"}
        };

        public static async Task<string> GetCCTVType(IOTOI.Model.CCTV cctv)
        {
            foreach (var IPCameraUri in IPCameraUris)
            {
                try
                {
                    using (HttpClient httpClient = new HttpClient())
                    {
                        httpClient.Timeout = TimeSpan.FromMilliseconds(2000);
                        httpClient.BaseAddress = new Uri("http://" + cctv.IpAddress);
                        string requestUri = String.Format(IPCameraUri.Value, cctv.AccountId, AESCipher.AES_Decrypt(cctv.AccountPass));

                        Debug.WriteLine("requestUri :: " + httpClient.BaseAddress + requestUri);

                        //Send the GET request
                        HttpResponseMessage httpResponse = await httpClient.GetAsync(requestUri);

                        if (httpResponse != null && httpResponse.StatusCode == HttpStatusCode.OK)
                        {
                            string ResponseText = await httpResponse.Content.ReadAsStringAsync();
                            XmlDocument xml = new XmlDocument();
                            xml.LoadXml(ResponseText);

                            if (xml.GetElementsByTagName("result")[0].InnerText == "0")
                            {
                                return IPCameraUri.Key;
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine("GetCCTVType Exception " + ex.Message);
                    continue;
                }
            }

            return "";
        }
    }
}
