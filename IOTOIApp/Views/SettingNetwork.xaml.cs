using IOTOIApp.ViewModels;
using Windows.ApplicationModel.Resources;
using Windows.Devices.WiFi;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace IOTOIApp.Views
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>
    public sealed partial class SettingNetwork : Page
    {
        private SettingNetworkViewModel ViewModel
        {
            get { return DataContext as SettingNetworkViewModel; }
        }

        public SettingNetwork()
        {
            this.InitializeComponent();
        }
        public void SetupNetwork()
        {
            ViewModel.WifiListView = WifiListView;
            ViewModel.SetupNetwork();
        }

        private void WifiPasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                ResourceLoader loader = ResourceLoader.GetForCurrentView();
                var expected = loader.GetString("Network_NetworkPasswordPrompt/Text");

                passwordBox.PlaceholderText = expected;

                passwordBox.Focus(FocusState.Programmatic);
            }
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;

            if (null == textBlock || null == textBlock.Tag)
            {
                return;
            }

            if ((textBlock.Tag as WiFiAvailableNetwork).SecuritySettings.NetworkEncryptionType == Windows.Networking.Connectivity.NetworkEncryptionType.None)
            {
                textBlock.Text = "";
            }
        }

    }
}
