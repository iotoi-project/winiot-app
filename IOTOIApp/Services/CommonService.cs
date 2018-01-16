using System;
using Windows.System;


using Windows.UI.Xaml;
using System.Threading.Tasks;

using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.Web.Http;
using Windows.Security.Cryptography;
using Windows.Web.Http.Headers;
using Newtonsoft.Json;


namespace IOTOIApp.Services
{
    class CommonService
    {
        public static void Shutdown()
        {
            ShutdownHelper(ShutdownKind.Shutdown);
        }

        public static void Restart()
        {
            ShutdownHelper(ShutdownKind.Restart);
        }

        public static void Close()
        {
             //Application.Current.Exit();
            Windows.UI.Xaml.Application.Current.Exit();
        }

        private static void ShutdownHelper(ShutdownKind kind)
        {
            new System.Threading.Tasks.Task(() =>
            {
                ShutdownManager.BeginShutdown(kind, TimeSpan.FromSeconds(0));
            }).Start();
        }

        public static async Task LaunchAppAsync(string uriStr)
        {
            Uri uri = new Uri(uriStr);
            var promptOptions = new Windows.System.LauncherOptions();
            promptOptions.TreatAsUntrusted = false;

            bool isSuccess = await Windows.System.Launcher.LaunchUriAsync(uri, promptOptions);

            if (!isSuccess)
            {
                string msg = "Launch failed";
                //await new MessageDialog(msg).ShowAsync();
            }
        }

        //public static async Task<IEnumerable<Models.InstalledPackage>> GetInstalledApplicationsModelDataAsync()
        //{
        //    await Task.CompletedTask;

        //    return await GetInstalledApplicationInfo();
        //}

        ///*
        //private IEnumerable<Models.Application> AllApplications()
        //{
        //    var data = 
        //}
        //*/
        ////private static async Task<IEnumerable<Models.InstalledPackage>> GetInstalledApplicationInfo()
        //private static async Task<IEnumerable<InstalledPackage>> GetInstalledApplicationInfo()
        //{
        //    //IList<InstalledPackage>
        //    //var data = new ObservableCollection<IEnumerable<Models.InstalledPackage>>();
        //    //var data = new ObservableCollection<IEnumerable<Models.InstalledPackage>>();
        //    //var data = new ObservableCollection<IEnumerable<Models.InstalledPackage>>();
        //    //var data = new ObservableCollection<List<Models.InstalledPackage>>();
        //    Models.Application application = new Models.Application();
        //    try
        //    {
        //        //var response = await getInstalledApplications("192.168.5.121", "administrator", "1234");
        //        var response = await getInstalledApplications("127.0.0.1", "administrator", "1234");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            //Models.Application application = new Models.Application();
        //            String js = await response.Content.ReadAsStringAsync();
                    
        //            application = JsonConvert.DeserializeObject<Models.Application>(js);
        //            //data = ObservableCollection<Models.Application>(JsonConvert.DeserializeObject<Models.Application>(js));
        //            //var result = JsonConvert.DeserializeObject(js);
        //            //data.Add(application.InstalledPackages);

        //        }
        //        else
        //        {
        //            //CmdEnabledStatus.Text = string.Format(resourceLoader.GetString("CmdTextEnabledFailure"), response.StatusCode);
        //            String elzB2 = "";
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        //e.Text = string.Format(resourceLoader.GetString("CmdTextEnabledFailure"), cmdEnabledException.HResult);
        //    }

        //    ;
        //    //return application;
        //    //return (IEnumerable<Models.InstalledPackage>)data;
        //    return application.InstalledPackages;

        //}

        //private static async Task<HttpResponseMessage> getInstalledApplications(string ipAddress, string username, string password)
        //{
        //    HttpClient client = new HttpClient();            
        //    var runAsdefault = CryptographicBuffer.ConvertStringToBinary("false", BinaryStringEncoding.Utf8);

        //    var urlContent = new HttpFormUrlEncodedContent(new[]
        //    {   
        //        new KeyValuePair<string,string>("runasdefaultaccount", CryptographicBuffer.EncodeToBase64String(runAsdefault)),
        //    });

        //    Uri uri = new Uri("http://" + ipAddress + ":8080/api/app/packagemanager/packages?" + await urlContent.ReadAsStringAsync());

        //    var authBuffer = CryptographicBuffer.ConvertStringToBinary(username + ":" + password, BinaryStringEncoding.Utf8);
        //    client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Basic", CryptographicBuffer.EncodeToBase64String(authBuffer));
            
        //    HttpResponseMessage response = await client.GetAsync(uri);

        //    return response;
        //}
    }
}
