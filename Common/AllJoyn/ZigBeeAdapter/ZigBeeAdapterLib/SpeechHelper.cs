using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace ZigbeeAdapterLib
{
    public sealed class SpeechHelper
    {
        private static SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        private static MediaElement mediaElement = new MediaElement();

        public static async void Speak(string textToSpeech)
        {
            if (!string.IsNullOrEmpty(textToSpeech))
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    VoiceInformation voiceInfo =
                      (
                        from voice in SpeechSynthesizer.AllVoices
                        where voice.Gender == VoiceGender.Female
                        select voice
                      ).FirstOrDefault() ?? SpeechSynthesizer.DefaultVoice;

                    speechSynthesizer.Voice = voiceInfo;

                    var speechSynthesisStream = await speechSynthesizer.SynthesizeTextToStreamAsync(textToSpeech);
                
                    mediaElement.SetSource(speechSynthesisStream, speechSynthesisStream.ContentType);
                    mediaElement.Play();
                });
            }
        }
    }
}
