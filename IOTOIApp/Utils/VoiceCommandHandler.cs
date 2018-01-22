using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace IOTOIApp.Utils
{
    class VoiceCommandHandler
    {
        //private AppServiceConnection connection = IOTOI.Shared.Model.Connection.connection;

        //VoiceCommandDefinition vd;
        static MediaElement mediaElement = new MediaElement();

        public async Task<string> SetPhraseList(string phraseList)
        {
            Debug.WriteLine("SetPhraseList START");
            string rtn = "ERROR";
            try
            {
                string countryCode = CultureInfo.CurrentCulture.Name.ToLower();

                if (countryCode.Length == 0)
                {
                    countryCode = "en-us";
                }

                //if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue("CommaxCommandSet_" + countryCode, out VoiceCommandDefinition vd))
                if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue("IOTOIAppCommandSet_" + countryCode,out VoiceCommandDefinition vd))
                {
                    Debug.WriteLine("VDM START");
                    //1. 이곳에서 DB조회 해오면 insert 메소드를 만들수 있음.
                    //여기서 몽땅 쿼리해서 넣도록 해야겠다.
                    List<string> targets = null;

                    targets = JsonConvert.DeserializeObject<List<string>>(phraseList);
                    if (targets.Count != 0)
                    {
                        Debug.WriteLine("SetPhraseListAsync START");
                        await vd.SetPhraseListAsync("target", targets);
                        Debug.WriteLine("SetPhraseListAsync END");
                        rtn = "OK";
                    }
                    else
                    {
                        rtn = "No Data";
                    }

                    
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            Debug.WriteLine("SetPhraseList COMPLETE");

            return rtn;
        }

        #region DoTTS
        public async void DoTTS(string txt)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    SpeechSynthesisStream speechSynthesisStream;

                    using (SpeechSynthesizer synthesizer = new SpeechSynthesizer())
                    {
                        // Our previous example effectively used SpeechSynthesizer.DefaultVoice
                        // Here we choose the first female voice on the system or we fallback
                        // to the default voice again.
                        VoiceInformation voiceInfo =
                          (
                            from voice in SpeechSynthesizer.AllVoices
                            where voice.Gender == VoiceGender.Female
                            select voice
                          ).FirstOrDefault() ?? SpeechSynthesizer.DefaultVoice;

                        synthesizer.Voice = voiceInfo;

                        speechSynthesisStream = await synthesizer.SynthesizeTextToStreamAsync(txt);
                    }
                    mediaElement.SetSource(speechSynthesisStream, speechSynthesisStream.ContentType);
                    mediaElement.Play();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }


            });

        }
        #endregion

    }
}
