/*
using System;

using IOTOIApp.Services;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

using Windows.System.Threading;
using Windows.Foundation;
using System.Diagnostics;
using AdapterLib;
using Windows.Storage;
using AdapterLib.Model;
using Newtonsoft.Json;

using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel.VoiceCommands;

using TestWorks.VoiceCommands;
*/

using System;


using IOTOIApp.Services;

using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

using Windows.System.Threading;
using Windows.Foundation;
using System.Diagnostics;
//using AdapterLib;
using Windows.Storage;
using Newtonsoft.Json;


using System.Linq;
using Newtonsoft.Json.Linq;
using Windows.ApplicationModel.VoiceCommands;
using System.IO;

//using TestWorks.VoiceCommands;
//using IOTOIApp.Model;

using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Microsoft.Practices.ServiceLocation;
using IOTOIApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using IOTOI.Model.Db;

namespace IOTOIApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private Lazy<ActivationService> _activationService;
        private ActivationService ActivationService { get { return _activationService.Value; } }
        private Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
        
        private AppServiceConnection _appServiceConnection;
        private BackgroundTaskDeferral _appServiceDeferral;

        private Utils.VoiceCommandHandler VoiceCommandHandler = new Utils.VoiceCommandHandler();        

        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }


        // Don't try and make discoverable if this has already been done
        private static bool isDiscoverable = false;

        public static bool IsBluetoothDiscoverable
        {
            get
            {
                return isDiscoverable;
            }

            set
            {
                isDiscoverable = value;
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Debug.WriteLine("App()");
            InitializeComponent();
            
            CommonInit();

            //Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);

            using (var db = new Context())
            {
                db.Database.Migrate();
            }
        }

        private async void CommonInit()
        {
            Debug.WriteLine("CommonInit()");
            #region ZigBee Adapter Init
            var message = new ValueSet();
            message.Add("Command", "Init");
            message.Add("Type", "Common");

            var rtnMessage = new ValueSet();
            await Task.Run(() => {
                Debug.WriteLine("Start Init");
                rtnMessage = IOTOI.Common.CommonService.GetReturnData(message);
                string rst = "CommonService Init ERROR";
                if (rtnMessage["Status"].ToString() == "OK")
                {
                    rst = "CommonService Init OK";
                    
                }

                Debug.WriteLine(rst);
                Debug.WriteLine("End Init");
            });
            if (rtnMessage["Status"].ToString() == "OK")
            {
                Debug.WriteLine("Start Footer");
                var FooterVM = ServiceLocator.Current.GetInstance<FooterViewModel>();
                FooterVM.CheckZigbeeAccess();
                Debug.WriteLine("End Footer");
            }
                
            #endregion

            #region Cortana Phrase List
            message = new ValueSet();
            message.Add("Command", "GetPhraseList");
            message.Add("Type", "Common");

            rtnMessage = IOTOI.Common.CommonService.GetReturnData(message);

            if (rtnMessage.ContainsKey("Result"))
            {
                await VoiceCommandHandler.SetPhraseList(rtnMessage["Result"] as string);
            }
            #endregion
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Debug.WriteLine("OnLaunched()");
            if (!e.PrelaunchActivated)
            {
                await ActivationService.ActivateAsync(e); 
            }

            #region VCD Install
            try
            {
                // Install the main VCD. Since there's no simple way to test that the VCD has been imported, or that it's your most recent
                // version, it's not unreasonable to do this upon app load.
                StorageFile vcdStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"IOTOIAppCommands.xml");

                await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcdStorageFile);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Installing Voice Commands Failed: " + ex.ToString());
            }
            #endregion
        }

        /// <summary>
        /// Invoked when the application is activated by some means other than normal launching.
        /// </summary>
        /// <param name="args">Event data for the event.</param>
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            Debug.WriteLine("OnActivated()");
            //isActivating = true;
            if (args.Kind == ActivationKind.VoiceCommand)
            {
                // The arguments can represent many different activation types. Cast it so we can get the
                // parameters we care about out.
                var commandArgs = args as VoiceCommandActivatedEventArgs;

                Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = commandArgs.Result;

                // Get the name of the voice command and the text spoken. See AdventureWorksCommands.xml for
                // the <Command> tags this can be filled with.
                string voiceCommandName = speechRecognitionResult.RulePath[0];
                string textSpoken = speechRecognitionResult.Text;

                //Action(textSpoken);
                /*
                switch (voiceCommandName)
                {
                    case "lockDevice":
                        Action(textSpoken);
                        break;
                    
                    default:
                        // If we can't determine what page to launch, go to the default entry point.

                        break;
                }
                */

            }
            else if (args.Kind == ActivationKind.Protocol)
            {


                //var commandArgs = args as ProtocolActivatedEventArgs;
                //Windows.Foundation.WwwFormUrlDecoder decoder = new Windows.Foundation.WwwFormUrlDecoder(commandArgs.Uri.Query);
                //var param = decoder.GetFirstValueByName("LaunchContext");
                //EndDevice test = JsonConvert.DeserializeObject<EndDevice>(param);

                //XBeeAction.PowerOff(Convert.ToUInt64(test.MacAddress), test.EndPointId);

                //var protocolEventArgs = args as ProtocolActivatedEventArgs;

                //switch (protocolEventArgs.Uri.Scheme)
                //{
                //    case "main-launchapplist":
                //        NavigationService.Navigate("IOTOIApp.ViewModels.AppListViewModel", "");
                //        break;
                //}

            }
            else
            {
                await ActivationService.ActivateAsync(args);
            }            
        }

            
        private ActivationService CreateActivationService()
        {
            Debug.WriteLine("CreateActivationService()");
            return new ActivationService(this, typeof(ViewModels.MainViewModel), new Views.HeaderPage());
        }
        

        #region OnBackGround
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            Debug.WriteLine("OnBackgroundActivated()");
            base.OnBackgroundActivated(args);
            IBackgroundTaskInstance taskInstance = args.TaskInstance;
            AppServiceTriggerDetails appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            _appServiceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += OnAppServicesCanceled;
            _appServiceConnection = appService.AppServiceConnection;
            _appServiceConnection.RequestReceived += OnAppServiceRequestReceived;
            _appServiceConnection.ServiceClosed += AppServiceConnection_ServiceClosed;
        }

        private async void OnAppServiceRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            Debug.WriteLine("OnAppServiceRequestReceived()");
            AppServiceDeferral messageDeferral = args.GetDeferral();
            ValueSet message = args.Request.Message;
            try
            {
                string text = message["Command"] as string;

                //1. CORTANA 명령이 들어왔을 경우
                ValueSet returnMessage = new ValueSet();
                if (text.ToUpper() == "CORTANA")
                {
                    Debug.WriteLine("Start SetPhraseList");
                    returnMessage.Add("Status", VoiceCommandHandler.SetPhraseList(message["PHRASELIST"] as string).Result);
                    Debug.WriteLine("End SetPhraseList");
                }
                else if (text.ToUpper() == "TTS")
                {
                    Debug.WriteLine("Start TTS");
                    Debug.WriteLine(message["Param"] as string);
                    VoiceCommandHandler.DoTTS(message["Param"] as string);
                }else if(text.ToUpper() == "LAUNCHNEST")
                {
                    Debug.WriteLine("Start LAUNCH");
                    var AppListVM = ServiceLocator.Current.GetInstance<AppListViewModel>();
                    AppListVM.AppLaunch("thermostat-launchmainpage://HostMainpage/Path1?param=" + message["Param"] as string);
                }
                else
                {
                    returnMessage = IOTOI.Common.CommonService.GetReturnData(message);

                }

                await args.Request.SendResponseAsync(returnMessage);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                messageDeferral.Complete();
            }

            //AppServiceDeferral messageDeferral = args.GetDeferral();
            //ValueSet message = args.Request.Message;
            //try
            //{
            //    string text = message["Command"] as string;

            //    if (text.ToUpper() == "CORTANA")
            //    {
            //        if (!await IOTOI.Shared.Model.Connection.GetConnection()) return;
            //        #region Cortana Phrase List
            //        message = new ValueSet();
            //        message.Add("Command", "GetPhraseList");
            //        message.Add("Type", "Common");

            //        //response = await connection.SendMessageAsync(message);
            //        AppServiceResponse response = await connection.SendMessageAsync(message);
            //        //List<string> targets = null;
            //        if (response.Status == AppServiceResponseStatus.Success && response.Message.ContainsKey("Result"))
            //        {
            //            //await Utils.VoiceCommandHandler.SetPhraseList(response.Message["Result"] as string);
            //            await VoiceCommandHandler.SetPhraseList(response.Message["Result"] as string);
            //        }
            //        #endregion
            //    }
                
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e.Message);
            //}
            //finally
            //{
            //    messageDeferral.Complete();
            //}


        }

        private void OnAppServicesCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            Debug.WriteLine("OnAppServicesCanceled()");
            _appServiceDeferral.Complete();
        }

        private void AppServiceConnection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Debug.WriteLine("AppServiceConnection_ServiceClosed()");
            _appServiceDeferral.Complete();
        }
        
        #endregion

    }
}
