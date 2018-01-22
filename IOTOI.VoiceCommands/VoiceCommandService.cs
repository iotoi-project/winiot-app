using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using unirest_net.http;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation.Collections;

namespace IOTOI.VoiceCommands
{
    public sealed class VoiceCommandService : IBackgroundTask
    {
        VoiceCommandServiceConnection voiceServiceConnection;
        BackgroundTaskDeferral serviceDeferral;
        ResourceMap cortanaResourceMap;
        ResourceContext cortanaContext;
        DateTimeFormatInfo dateFormatInfo;

        private AppServiceConnection _connection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();

            taskInstance.Canceled += OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            // Load localized resources for strings sent to Cortana to be displayed to the user.
            cortanaResourceMap = ResourceManager.Current.MainResourceMap.GetSubtree("Resources");

            // Select the system language, which is what Cortana should be running as.
            cortanaContext = ResourceContext.GetForViewIndependentUse();

            // Get the currently used system date format
            dateFormatInfo = CultureInfo.CurrentCulture.DateTimeFormat;

            if (triggerDetails != null && triggerDetails.Name == "VoiceCommandService")
            {
                Debug.WriteLine(triggerDetails.Name);
                try
                {
                    voiceServiceConnection =
                        VoiceCommandServiceConnection.FromAppServiceTriggerDetails(
                            triggerDetails);

                    voiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                    // GetVoiceCommandAsync establishes initial connection to Cortana, and must be called prior to any 
                    // messages sent to Cortana. Attempting to use ReportSuccessAsync, ReportProgressAsync, etc
                    // prior to calling this will produce undefined behavior.
                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                    // Depending on the operation (defined in AdventureWorks:AdventureWorksCommands.xml)
                    // perform the appropriate command.
                    Debug.WriteLine(voiceCommand.CommandName);


                    switch (voiceCommand.CommandName)
                    {
                        case "PowerOnOff":
                            //await ReportStatus(voiceCommand.Properties["device"][0]);
                            var PowerTarget = voiceCommand.Properties["target"][0];
                            var onoff = voiceCommand.Properties["onoff"][0];
                            await SendCompletionMessageForPowerOnOff(PowerTarget, onoff);
                            break;
                        case "reportStatus":
                            var ReportTarget = voiceCommand.Properties["target"][0];
                            await SendCompletionMessageForReport(ReportTarget);
                            break;
                        case "reportAppointment":
                            var AppointmentTarget = voiceCommand.Properties["Days"][0];
                            await SendCompletionMessageForAppointmentReport(AppointmentTarget);
                            break;
                        case "SetTemperature":
                            var Temperature = voiceCommand.SpeechRecognitionResult.SemanticInterpretation.Properties["TemperatureNumber"].FirstOrDefault().ToString();
                            await SendCompletionMessageForSetTemperature(Temperature);
                            break;
                        case "StartFan":
                            //var startmin = voiceCommand.SpeechRecognitionResult.SemanticInterpretation.Properties["TemperatureNumber"].FirstOrDefault().ToString();
                            var startmin = voiceCommand.Properties["fanstartminutes"][0];
                            await SendCompletionMessageForFan(false, startmin);
                            break;
                        case "StopFan":
                            await SendCompletionMessageForFan(true, string.Empty);
                            break;
                        case "AskNest":
                            var NestTarget = voiceCommand.Properties["NestDevice"][0];
                            await SendCompletionMessageForAskNest(NestTarget);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Handling Voice Command failed " + ex.ToString());
                }
                finally
                {
                    if (this.serviceDeferral != null)
                    {
                        // Complete the service deferral.
                        this.serviceDeferral.Complete();
                    }
                }
            }
        }

        #region SendCompletionMessageForReport
        private async Task SendCompletionMessageForReport(string target)
        {
            #region ShowProgressScreen
            // If this operation is expected to take longer than 0.5 seconds, the task must
            // provide a progress response to Cortana prior to starting the operation, and
            // provide updates at most every 5 seconds.
            try
            {
                string loadingPowerOnOff = string.Format(
                       cortanaResourceMap.GetValue("LoadingReport", cortanaContext).ValueAsString,
                       target);
                await ShowProgressScreen(loadingPowerOnOff);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }



            #endregion

            var userMessage = new VoiceCommandUserMessage();
            var destinationsContentTiles = new List<VoiceCommandContentTile>();

            #region checkConnection
            //if (_connection == null)
            //{
            //    Debug.WriteLine("_connection is null");
            //    _connection = new AppServiceConnection();

            //    _connection.AppServiceName = "VoiceCommandService";
            //    _connection.PackageFamilyName = "CommaxIotApp_5s72hevbe334y";

            //    var status = await _connection.OpenAsync();

            //    switch (status)
            //    {
            //        case AppServiceConnectionStatus.AppNotInstalled:
            //            Debug.WriteLine("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.");
            //            _connection = null;
            //            break;

            //        case AppServiceConnectionStatus.AppUnavailable:
            //            Debug.WriteLine("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.");
            //            _connection = null;
            //            break;

            //        case AppServiceConnectionStatus.AppServiceUnavailable:
            //            Debug.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", _connection.AppServiceName));
            //            _connection = null;
            //            break;

            //        case AppServiceConnectionStatus.Unknown:
            //            Debug.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
            //            _connection = null;
            //            break;
            //    }
            //}
            #endregion

            
            #region Logic
            if (_connection == null)
            {
                #region CommonService 검색실패시
                //userMessage = new VoiceCommandUserMessage();
                //destinationsContentTiles = new List<VoiceCommandContentTile>();
                if (true)
                {
                    // In this scenario, perhaps someone has modified data on your service outside of your 
                    // control. If you're accessing a remote service, having a background task that
                    // periodically refreshes the phrase list so it's likely to be in sync is ideal.
                    // This is unlikely to occur for this sample app, however.
                    string foundNoTargetToPowerHandle = string.Format(
                           cortanaResourceMap.GetValue("FoundNoTargetToPowerHandle", cortanaContext).ValueAsString,
                           target);
                    userMessage.DisplayMessage = foundNoTargetToPowerHandle;
                    userMessage.SpokenMessage = foundNoTargetToPowerHandle;
                }
                #endregion
            }
            else
            {
                var message = new ValueSet();

                message.Add("Type", "Common");
                message.Add("Command", "GetStatus");
                message.Add("Target", target);

                AppServiceResponse responseData = await _connection.SendMessageAsync(message);

                bool isOpen = false;

                if (responseData.Status == AppServiceResponseStatus.Success && responseData.Message.ContainsKey("Result"))
                {
                    isOpen = (bool)responseData.Message["Result"];
                }

                string strOpen = (isOpen) ? "On" : "Off";

                userMessage.DisplayMessage = string.Format(cortanaResourceMap.GetValue("ReportStatus", cortanaContext).ValueAsString, target, strOpen);
                userMessage.SpokenMessage = string.Format(cortanaResourceMap.GetValue("ReportStatus", cortanaContext).ValueAsString, target, strOpen);


                //var response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);
                var response = VoiceCommandResponse.CreateResponse(userMessage);

                await voiceServiceConnection.ReportSuccessAsync(response);
            }

            #endregion
        }
        #endregion

        #region SendCompletionMessageForPowerOnOff
        private async Task SendCompletionMessageForPowerOnOff(string target, string onoff)
        {
            #region ShowProgressScreen
            // If this operation is expected to take longer than 0.5 seconds, the task must
            // provide a progress response to Cortana prior to starting the operation, and
            // provide updates at most every 5 seconds.
            try
            {
                string loadingPowerOnOff = string.Format(
                       cortanaResourceMap.GetValue("LoadingPowerHandling", cortanaContext).ValueAsString,
                       target);
                await ShowProgressScreen(loadingPowerOnOff);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }



            #endregion

            var userMessage = new VoiceCommandUserMessage();
            var destinationsContentTiles = new List<VoiceCommandContentTile>();

            #region checkConnection
            if (_connection == null)
            {
                Debug.WriteLine("_connection is null");
                _connection = new AppServiceConnection();

                _connection.AppServiceName = "VoiceCommandService";
                _connection.PackageFamilyName = "IOTOIApp_5s72hevbe334y";

                var status = await _connection.OpenAsync();

                switch (status)
                {
                    case AppServiceConnectionStatus.AppNotInstalled:
                        Debug.WriteLine("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppUnavailable:
                        Debug.WriteLine("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppServiceUnavailable:
                        Debug.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", _connection.AppServiceName));
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.Unknown:
                        Debug.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
                        _connection = null;
                        break;
                }
            }
            #endregion

            #region Logic
            if (_connection == null)
            {
                #region CommonService 검색실패시
                //userMessage = new VoiceCommandUserMessage();
                //destinationsContentTiles = new List<VoiceCommandContentTile>();
                if (true)
                {
                    // In this scenario, perhaps someone has modified data on your service outside of your 
                    // control. If you're accessing a remote service, having a background task that
                    // periodically refreshes the phrase list so it's likely to be in sync is ideal.
                    // This is unlikely to occur for this sample app, however.
                    string foundNoTargetToPowerHandle = string.Format(
                           cortanaResourceMap.GetValue("FoundNoTargetToPowerHandle", cortanaContext).ValueAsString,
                           target);
                    //userMessage.DisplayMessage = foundNoTargetToPowerHandle;
                    userMessage.SpokenMessage = foundNoTargetToPowerHandle;
                }
                #endregion
            }
            else
            {
                var message = new ValueSet();

                message.Add("Type", "Common");
                message.Add("Command", "GetDevice");
                message.Add("Target", target);

                AppServiceResponse responseData = await _connection.SendMessageAsync(message);

                string endPoint = null;
                if (responseData.Status == AppServiceResponseStatus.Success && responseData.Message.ContainsKey("Result"))
                {
                    //id = Convert.ToInt32(responseData.Message["Result"]);
                    endPoint = responseData.Message["Result"] as string;
                }

                if (endPoint != null)
                {
                    userMessage.DisplayMessage = string.Format(cortanaResourceMap.GetValue("DoingPowerHandling", cortanaContext).ValueAsString, target);
                    userMessage.SpokenMessage = string.Format(cortanaResourceMap.GetValue("DoingPowerHandling", cortanaContext).ValueAsString, target);

                    message = new ValueSet();
                    message.Add("Type", "ZigBee");
                    message.Add("endPoint", endPoint);

                    var destinationTile = new VoiceCommandContentTile();
                    destinationTile.ContentTileType = VoiceCommandContentTileType.TitleWithText;
                    //string rtnTitle = "The {0} is "
                    destinationTile.Title = "Result";
                    if (onoff.ToUpper() == "ON")
                    {
                        message.Add("Command", "PLUGPOWERON");
                        destinationTile.TextLine1 = string.Format("The {0} is On", target);
                    }
                    else
                    {
                        message.Add("Command", "PLUGPOWEROFF");
                        destinationTile.TextLine1 = string.Format("The {0} is Off", target);
                    }
                    destinationsContentTiles.Add(destinationTile);
                    await _connection.SendMessageAsync(message);
                }
                else
                {
                    string foundNoTargetToPowerHandle = string.Format(
                          cortanaResourceMap.GetValue("FoundNoTargetToPowerHandle", cortanaContext).ValueAsString,
                          target);
                    userMessage.DisplayMessage = foundNoTargetToPowerHandle;
                    userMessage.SpokenMessage = foundNoTargetToPowerHandle;
                }
            }
            //Common으로 Method와 target을 날려 성공여부를 리턴 받아 후처리
            //1. Found EndPoint Id

            /*
             * select z.Id from ZigBeeEndPoint z join "Space" s on z.SpaceId = s.Id
where Upper(replace(s.Name||z.CustomName, ' ', '')) = 'LIVINGROOMLAMP';
             */

            //2. Action
            #endregion

            //#region 장비 검색 실패시.
            //var userMessage = new VoiceCommandUserMessage();
            //var destinationsContentTiles = new List<VoiceCommandContentTile>();
            //if (true)
            //{
            //    // In this scenario, perhaps someone has modified data on your service outside of your 
            //    // control. If you're accessing a remote service, having a background task that
            //    // periodically refreshes the phrase list so it's likely to be in sync is ideal.
            //    // This is unlikely to occur for this sample app, however.
            //    string foundNoTargetToPowerHandle = string.Format(
            //           cortanaResourceMap.GetValue("FoundNoTargetToPowerHandle", cortanaContext).ValueAsString,
            //           target);
            //    userMessage.DisplayMessage = foundNoTargetToPowerHandle;
            //    userMessage.SpokenMessage = foundNoTargetToPowerHandle;
            //}
            //#endregion


            var response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);
            //var response = VoiceCommandResponse.CreateResponse(userMessage);

            await voiceServiceConnection.ReportSuccessAsync(response);

        }
        #endregion

        #region SendCompletionMessageForAppointmentReport
        private async Task SendCompletionMessageForAppointmentReport(string target)
        {
            #region ShowProgressScreen
            // If this operation is expected to take longer than 0.5 seconds, the task must
            // provide a progress response to Cortana prior to starting the operation, and
            // provide updates at most every 5 seconds.
            try
            {
                string loadingPowerOnOff = string.Format(
                       cortanaResourceMap.GetValue("LoadingReport", cortanaContext).ValueAsString,
                       target);
                await ShowProgressScreen(loadingPowerOnOff);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }



            #endregion

            var userMessage = new VoiceCommandUserMessage();
            var destinationsContentTiles = new List<VoiceCommandContentTile>();

            #region Logic
            UriBuilder baseUri = new UriBuilder("https://outlook.office365.com/api/v1.0/me/calendarview");
            var today = DateTime.Now.Date;
            var yesterday = today.AddDays(-1);
            var tommorw = today.AddDays(1);
            string strDate = "";
            string endDate = "";
            switch (target.ToUpper())
            {
                case "TODAY":
                    strDate = today.ToString("yyyy-MM-dd");
                    endDate = strDate + " 23:59:59";
                    break;
                case "YESTERDAY":
                    strDate = yesterday.ToString("yyyy-MM-dd");
                    endDate = strDate + " 23:59:59";
                    break;
                case "TOMMOROW":
                    strDate = tommorw.ToString("yyyy-MM-dd");
                    endDate = strDate + " 23:59:59";
                    break;
                default:
                    break;
            }


            string queryToAppend = "startDateTime=" + strDate + "&endDateTime=" + endDate;
            queryToAppend += "&$select=Subject,Location,Start,End";
            baseUri.Query = queryToAppend;

            //Set Id/PW
            HttpResponse<string> restresponse = Unirest.get(baseUri.ToString()).basicAuth(@"cesdemo@domain.co.kr", "password").asJson<string>();
            if (restresponse.Code == 200)
            {
                foreach (JObject jb in JObject.Parse(restresponse.Body)["value"])
                {
                    var dest = new VoiceCommandContentTile();
                    dest.ContentTileType = VoiceCommandContentTileType.TitleWithText;
                    dest.Title = jb["Subject"].ToString();

                    DateTime dts = Convert.ToDateTime(jb["Start"].ToString());
                    DateTime dte = Convert.ToDateTime(jb["End"].ToString());

                    StringBuilder txtLine1 = new StringBuilder();
                    txtLine1.Append(dts.DayOfWeek);
                    txtLine1.Append(", ");
                    txtLine1.Append(dts.ToString("MMMM", CultureInfo.InvariantCulture));
                    txtLine1.Append(", ");
                    txtLine1.Append(dts.Day);
                    txtLine1.Append(", ");
                    txtLine1.Append(dts.Year);
                    dest.TextLine1 = txtLine1.ToString();

                    StringBuilder txtLine2 = new StringBuilder();
                    txtLine2.Append(dts.ToString("hh:mm tt", CultureInfo.InvariantCulture));
                    txtLine2.Append(" - ");
                    txtLine2.Append(dte.ToString("hh:mm tt", CultureInfo.InvariantCulture));
                    dest.TextLine2 = txtLine2.ToString();



                    if (jb["Location"]["DisplayName"] != null)
                    {
                        dest.TextLine3 = jb["Location"]["DisplayName"].ToString();
                    }

                    destinationsContentTiles.Add(dest);
                }
            }

            userMessage.DisplayMessage = "Here's your Event";
            userMessage.SpokenMessage = "Here's your Event";


            var response = VoiceCommandResponse.CreateResponse(userMessage, destinationsContentTiles);

            await voiceServiceConnection.ReportSuccessAsync(response);

            #endregion
        }
        #endregion

        #region SendCompletionMessageForSetTemperature
        private async Task SendCompletionMessageForSetTemperature(string target)
        {
            //TODO : Message 관련 미적용
            #region ShowProgressScreen
            // If this operation is expected to take longer than 0.5 seconds, the task must
            // provide a progress response to Cortana prior to starting the operation, and
            // provide updates at most every 5 seconds.
            try
            {
                string loadingPowerOnOff = string.Format(
                       cortanaResourceMap.GetValue("LoadingPowerHandling", cortanaContext).ValueAsString,
                       target);
                await ShowProgressScreen(loadingPowerOnOff);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }



            #endregion

            var userMessage = new VoiceCommandUserMessage();
            var destinationsContentTiles = new List<VoiceCommandContentTile>();

            #region checkConnection
            if (_connection == null)
            {
                Debug.WriteLine("_connection is null");
                _connection = new AppServiceConnection();

                _connection.AppServiceName = "VoiceCommandService";
                _connection.PackageFamilyName = "IOTOIApp_5s72hevbe334y";

                var status = await _connection.OpenAsync();

                switch (status)
                {
                    case AppServiceConnectionStatus.AppNotInstalled:
                        Debug.WriteLine("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppUnavailable:
                        Debug.WriteLine("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppServiceUnavailable:
                        Debug.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", _connection.AppServiceName));
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.Unknown:
                        Debug.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
                        _connection = null;
                        break;
                }
            }
            #endregion

            #region Validation Temp
            int Temp = 0;
            bool result = int.TryParse(target, out Temp);
            #endregion

            if (!result)
            {
                string foundNoTargetToPowerHandle = "Invalid temperature";
                userMessage.DisplayMessage = foundNoTargetToPowerHandle;
                userMessage.SpokenMessage = foundNoTargetToPowerHandle;
            }
            else if (Temp < 50 || Temp > 90)
            {
                string foundNoTargetToPowerHandle = "Invalid temperature";
                userMessage.DisplayMessage = foundNoTargetToPowerHandle;
                userMessage.SpokenMessage = foundNoTargetToPowerHandle;
            }
            else
            {
                //string strResult = "Valid temperature";
                //userMessage.DisplayMessage = strResult;
                //userMessage.SpokenMessage = strResult;

                #region Logic
                if (_connection == null)
                {
                    #region CommonService 검색실패시
                    //userMessage = new VoiceCommandUserMessage();
                    //destinationsContentTiles = new List<VoiceCommandContentTile>();
                    if (true)
                    {
                        // In this scenario, perhaps someone has modified data on your service outside of your 
                        // control. If you're accessing a remote service, having a background task that
                        // periodically refreshes the phrase list so it's likely to be in sync is ideal.
                        // This is unlikely to occur for this sample app, however.
                        string foundNoTargetToPowerHandle = string.Format(
                               cortanaResourceMap.GetValue("FoundNoTargetToPowerHandle", cortanaContext).ValueAsString,
                               target);
                        userMessage.DisplayMessage = foundNoTargetToPowerHandle;
                        userMessage.SpokenMessage = foundNoTargetToPowerHandle;
                    }
                    #endregion
                }
                else
                {
                    var message = new ValueSet();

                    message.Add("Command", "LAUNCHNEST");
                    message.Add("Param", "target_temperature_f " + Temp);

                    AppServiceResponse responseData = await _connection.SendMessageAsync(message);

                    if (responseData.Status == AppServiceResponseStatus.Success)
                    {
                        string strResult = "Valid temperature";
                        userMessage.DisplayMessage = strResult;
                        userMessage.SpokenMessage = strResult;
                    }

                }

                #endregion
            }

            #region checkConnection
            if (_connection == null)
            {
                Debug.WriteLine("_connection is null");
                _connection = new AppServiceConnection();

                _connection.AppServiceName = "VoiceCommandService";
                _connection.PackageFamilyName = "IOTOIApp_5s72hevbe334y";

                var status = await _connection.OpenAsync();

                switch (status)
                {
                    case AppServiceConnectionStatus.AppNotInstalled:
                        Debug.WriteLine("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppUnavailable:
                        Debug.WriteLine("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppServiceUnavailable:
                        Debug.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", _connection.AppServiceName));
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.Unknown:
                        Debug.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
                        _connection = null;
                        break;
                }
            }
            #endregion




            var response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportSuccessAsync(response);

        }
        #endregion

        #region SendCompletionMessageForAskNest
        private async Task SendCompletionMessageForAskNest(string target)
        {
            //TODO : Message 관련 미적용
            #region ShowProgressScreen
            // If this operation is expected to take longer than 0.5 seconds, the task must
            // provide a progress response to Cortana prior to starting the operation, and
            // provide updates at most every 5 seconds.
            try
            {
                string loadingPowerOnOff = string.Format(
                       cortanaResourceMap.GetValue("LoadingPowerHandling", cortanaContext).ValueAsString,
                       target);
                await ShowProgressScreen(loadingPowerOnOff);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }



            #endregion

            var userMessage = new VoiceCommandUserMessage();
            var destinationsContentTiles = new List<VoiceCommandContentTile>();

            #region checkConnection
            if (_connection == null)
            {
                Debug.WriteLine("_connection is null");
                _connection = new AppServiceConnection();

                _connection.AppServiceName = "VoiceCommandService";
                _connection.PackageFamilyName = "IOTOIApp_5s72hevbe334y";

                var status = await _connection.OpenAsync();

                switch (status)
                {
                    case AppServiceConnectionStatus.AppNotInstalled:
                        Debug.WriteLine("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppUnavailable:
                        Debug.WriteLine("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppServiceUnavailable:
                        Debug.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", _connection.AppServiceName));
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.Unknown:
                        Debug.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
                        _connection = null;
                        break;
                }
            }
            #endregion

            #region Logic
            if (_connection == null)
            {
                #region CommonService 검색실패시
                //userMessage = new VoiceCommandUserMessage();
                //destinationsContentTiles = new List<VoiceCommandContentTile>();
                if (true)
                {
                    // In this scenario, perhaps someone has modified data on your service outside of your 
                    // control. If you're accessing a remote service, having a background task that
                    // periodically refreshes the phrase list so it's likely to be in sync is ideal.
                    // This is unlikely to occur for this sample app, however.
                    string foundNoTargetToPowerHandle = string.Format(
                           cortanaResourceMap.GetValue("FoundNoTargetToPowerHandle", cortanaContext).ValueAsString,
                           target);
                    userMessage.DisplayMessage = foundNoTargetToPowerHandle;
                    userMessage.SpokenMessage = foundNoTargetToPowerHandle;
                }
                #endregion
            }
            else
            {
                var message = new ValueSet();

                message.Add("Type", "Common");
                message.Add("Command", "GetNestStatus");
                message.Add("Target", target);

                AppServiceResponse responseData = await _connection.SendMessageAsync(message);

                //bool isOpen = false;
                string strResult = null;

                if (responseData.Status == AppServiceResponseStatus.Success && responseData.Message.ContainsKey("Status"))
                {
                    //isOpen = (bool)responseData.Message["Result"];
                    if (target == "fan")
                    {
                        if (responseData.Message["Status"].ToString() != "OK")
                        {
                            strResult = "Fail";
                        }
                        else
                        {
                            strResult = (bool)responseData.Message["Result"] ? "On" : "Off";
                        }

                    }
                    else
                    {
                        if (responseData.Message["Status"].ToString() != "OK")
                        {
                            strResult = "Fail";
                        }
                        else
                        {
                            strResult = responseData.Message["Result"].ToString();
                        }

                    }
                }


                //userMessage.DisplayMessage = string.Format(cortanaResourceMap.GetValue("ReportStatus", cortanaContext).ValueAsString, target, strResult);
                //userMessage.SpokenMessage = string.Format(cortanaResourceMap.GetValue("ReportStatus", cortanaContext).ValueAsString, target, strResult);
                userMessage.DisplayMessage = "The Result is " + strResult;
                userMessage.SpokenMessage = "The Result is " + strResult;

                var response = VoiceCommandResponse.CreateResponse(userMessage);

                await voiceServiceConnection.ReportSuccessAsync(response);
            }

            #endregion

        }
        #endregion

        #region SendCompletionMessageForSetTemperature
        private async Task SendCompletionMessageForFan(bool isStop, string target)
        {
            //TODO : Message 관련 미적용
            #region ShowProgressScreen
            // If this operation is expected to take longer than 0.5 seconds, the task must
            // provide a progress response to Cortana prior to starting the operation, and
            // provide updates at most every 5 seconds.
            try
            {
                string loadingPowerOnOff = string.Format(
                       cortanaResourceMap.GetValue("LoadingPowerHandling", cortanaContext).ValueAsString,
                       target);
                await ShowProgressScreen(loadingPowerOnOff);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }



            #endregion

            var userMessage = new VoiceCommandUserMessage();
            var destinationsContentTiles = new List<VoiceCommandContentTile>();

            #region checkConnection
            if (_connection == null)
            {
                Debug.WriteLine("_connection is null");
                _connection = new AppServiceConnection();

                _connection.AppServiceName = "VoiceCommandService";
                _connection.PackageFamilyName = "IOTOIApp_5s72hevbe334y";

                var status = await _connection.OpenAsync();

                switch (status)
                {
                    case AppServiceConnectionStatus.AppNotInstalled:
                        Debug.WriteLine("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppUnavailable:
                        Debug.WriteLine("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppServiceUnavailable:
                        Debug.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", _connection.AppServiceName));
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.Unknown:
                        Debug.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
                        _connection = null;
                        break;
                }
            }
            #endregion

            #region Start일 경우
            if (!isStop)
            {

            }

            #endregion


            #region Logic
            if (_connection == null)
            {
                #region CommonService 검색실패시
                //userMessage = new VoiceCommandUserMessage();
                //destinationsContentTiles = new List<VoiceCommandContentTile>();
                if (true)
                {
                    // In this scenario, perhaps someone has modified data on your service outside of your 
                    // control. If you're accessing a remote service, having a background task that
                    // periodically refreshes the phrase list so it's likely to be in sync is ideal.
                    // This is unlikely to occur for this sample app, however.
                    string foundNoTargetToPowerHandle = string.Format(
                           cortanaResourceMap.GetValue("FoundNoTargetToPowerHandle", cortanaContext).ValueAsString,
                           target);
                    userMessage.DisplayMessage = foundNoTargetToPowerHandle;
                    userMessage.SpokenMessage = foundNoTargetToPowerHandle;
                }
                #endregion
            }
            else
            {
                var message = new ValueSet();

                message.Add("Command", "LAUNCHNEST");
                if (isStop)
                {
                    message.Add("Param", "fan_timer_stop");
                }
                else
                {
                    message.Add("Param", "fan_timer_active " + Convert.ToInt16(target));
                }


                AppServiceResponse responseData = await _connection.SendMessageAsync(message);

                if (responseData.Status == AppServiceResponseStatus.Success)
                {
                    string strResult = "Valid temperature";
                    userMessage.DisplayMessage = strResult;
                    userMessage.SpokenMessage = strResult;
                }

            }

            #endregion

            #region checkConnection
            if (_connection == null)
            {
                Debug.WriteLine("_connection is null");
                _connection = new AppServiceConnection();

                _connection.AppServiceName = "VoiceCommandService";
                _connection.PackageFamilyName = "IOTOIApp_5s72hevbe334y";

                var status = await _connection.OpenAsync();

                switch (status)
                {
                    case AppServiceConnectionStatus.AppNotInstalled:
                        Debug.WriteLine("The app AppServicesProvider is not installed. Deploy AppServicesProvider to this device and try again.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppUnavailable:
                        Debug.WriteLine("The app AppServicesProvider is not available. This could be because it is currently being updated or was installed to a removable device that is no longer available.");
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.AppServiceUnavailable:
                        Debug.WriteLine(string.Format("The app AppServicesProvider is installed but it does not provide the app service {0}.", _connection.AppServiceName));
                        _connection = null;
                        break;

                    case AppServiceConnectionStatus.Unknown:
                        Debug.WriteLine("An unkown error occurred while we were trying to open an AppServiceConnection.");
                        _connection = null;
                        break;
                }
            }
            #endregion




            var response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceServiceConnection.ReportSuccessAsync(response);

        }
        #endregion

        #region ShowProgressScreen
        private async Task ShowProgressScreen(string message)
        {
            var userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = message;
            //userProgressMessage.DisplayMessage = message;

            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await voiceServiceConnection.ReportProgressAsync(response);
        }
        #endregion

        #region OnVoiceCommandCompleted
        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }
        #endregion

        #region OnTaskCanceled
        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            System.Diagnostics.Debug.WriteLine("Task cancelled, clean up");
            if (this.serviceDeferral != null)
            {
                //Complete the service deferral
                this.serviceDeferral.Complete();
            }
        }
        #endregion
    }
}
