using IOTOIApp.Helpers;
using IOTOIApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Cortana;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace IOTOIApp.Views
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class SettingBasicPreferences : Page
    {
        LanguageManager languageManager;

        string CurrentInputLang = "";


        public SettingBasicPreferences()
        {
            this.InitializeComponent();

            this.Loaded += SettingBasicPreferences_Loaded;
        }

        private void SettingBasicPreferences_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= SettingBasicPreferences_Loaded;

            languageManager = LanguageManager.GetInstance();

            LanguageComboBox.ItemsSource = languageManager.LanguageDisplayNames;
            LanguageComboBox.SelectedItem = LanguageManager.GetCurrentLanguageDisplayName();

            InputLanguageComboBox.ItemsSource = languageManager.InputLanguageDisplayNames;
            InputLanguageComboBox.SelectedItem = CurrentInputLang =LanguageManager.GetCurrentInputLanguageDisplayName();

            LanguageComboBox.SelectionChanged += LanguageComboBox_SelectionChanged;
            InputLanguageComboBox.SelectionChanged += InputLanguageComboBox_SelectionChanged;

            CheckCortana();

            SwitchCotana.Checked += SwitchCotana_Checked;
            SwitchCotana.Unchecked += SwitchCotana_Unchecked;
        }




        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            languageManager.UpdateLanguage(LanguageComboBox.SelectedItem as string);
        }


        private void InputLanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!languageManager.UpdateInputLanguage(InputLanguageComboBox.SelectedItem as string))
            {
                InputLanguageComboBox.SelectedItem = CurrentInputLang;
            }
        }


        private void SwitchCotana_Checked(object sender, RoutedEventArgs e)
        {
            CortanaVoiceActivation(true);
        }


        private void SwitchCotana_Unchecked(object sender, RoutedEventArgs e)
        {
            CortanaVoiceActivation(false);
        }


        void CheckCortana()
        {
            var isCortanaSupported = CortanaHelper.IsCortanaSupported();

            var cortanaVoiceActivationSwitch = SwitchCotana;

            cortanaVoiceActivationSwitch.IsEnabled = isCortanaSupported;

            // If Cortana is supported on this device and the user has never granted voice consent,
            // then set a flag so that each time this page is activated we will poll for
            // Cortana's Global Consent Value and update the UI if needed.
            if (isCortanaSupported)
            {
                var cortanaSettings = CortanaSettings.GetDefault();
                bool needsCortanaConsent = !cortanaSettings.HasUserConsentToVoiceActivation;

                // If consent isn't needed, then update the voice activation switch to reflect its current system state.
                if (!needsCortanaConsent)
                {
                    cortanaVoiceActivationSwitch.IsChecked = cortanaSettings.IsVoiceActivationEnabled;
                }
            }
        }


        async void CortanaVoiceActivation(bool active)
        {
            var cortanaSettings = CortanaSettings.GetDefault();
            var cortanaVoiceActivationSwitch = SwitchCotana;

            bool enableVoiceActivation = active;

            // If user is requesting to turn on voice activation, but consent has not been provided yet, then launch Cortana to ask for consent first
            if (!cortanaSettings.HasUserConsentToVoiceActivation)
            {
                // Guard against the case where the switch is toggled off when Consent hasn't been given yet
                // This occurs when we are re-entering this method when the switch is turned off in the code that follows
                if (!enableVoiceActivation)
                {
                    return;
                }

                cortanaVoiceActivationSwitch.IsEnabled = false;
                cortanaVoiceActivationSwitch.IsChecked = false;
            }
            // Otherwise, we already have consent, so just enable or disable the voice activation setting.
            // Do this asynchronously because the API waits for the SpeechRuntime EXE to launch
            else
            {
                cortanaVoiceActivationSwitch.IsEnabled = false;
                await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await SetVoiceActivation(enableVoiceActivation);
                    cortanaVoiceActivationSwitch.IsEnabled = true;
                });
            }
        }


        const int RPC_S_CALL_FAILED = -2147023170;
        const int RPC_S_SERVER_UNAVAILABLE = -2147023174;
        const int RPC_S_SERVER_TOO_BUSY = -2147023175;
        const int MAX_VOICEACTIVATION_TRIALS = 5;
        const int TIMEINTERVAL_VOICEACTIVATION = 10;    // milli sec
        private async Task SetVoiceActivation(bool value)
        {
            var cortanaSettings = CortanaSettings.GetDefault();
            for (int i = 0; i < MAX_VOICEACTIVATION_TRIALS; i++)
            {
                try
                {
                    cortanaSettings.IsVoiceActivationEnabled = value;
                }
                catch (System.Exception ex)
                {
                    if (ex.HResult == RPC_S_CALL_FAILED ||
                        ex.HResult == RPC_S_SERVER_UNAVAILABLE ||
                        ex.HResult == RPC_S_SERVER_TOO_BUSY)
                    {
                        // VoiceActivation server is very likely busy =>
                        // yield and take a new ref to CortanaSettings API
                        await Task.Delay(TimeSpan.FromMilliseconds(TIMEINTERVAL_VOICEACTIVATION));
                        cortanaSettings = CortanaSettings.GetDefault();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }
        }
    }
}
