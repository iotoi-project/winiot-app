using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.WiFi;
using Windows.Foundation;
using Windows.Security.Credentials;
using Windows.Services.Cortana;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Radios;
using Windows.Devices.Bluetooth;
using System.Linq;
using IOTOIApp.ViewModels;

namespace IOTOIApp.Views
{
    public sealed partial class SettingsPage : Page
    {
        //private LanguageManager languageManager;
        private UIElement visibleContent;
        private NetworkPresenter networkPresenter = new NetworkPresenter();
        private bool Automatic = true;
        private string CurrentPassword = string.Empty;
        // Device watcher
        private DeviceWatcher deviceWatcher = null;
        private TypedEventHandler<DeviceWatcher, DeviceInformation> handlerAdded = null;
        private TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> handlerUpdated = null;
        private TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> handlerRemoved = null;
        private TypedEventHandler<DeviceWatcher, Object> handlerEnumCompleted = null;
        private TypedEventHandler<DeviceWatcher, Object> handlerStopped = null;
        // Pairing controls and notifications
        private enum MessageType { YesNoMessage, OKMessage, InformationalMessage };
        Windows.Devices.Enumeration.DevicePairingRequestedEventArgs pairingRequestedHandlerArgs;
        Windows.Foundation.Deferral deferral;
        Windows.Devices.Bluetooth.Rfcomm.RfcommServiceProvider provider = null; // To be used for inbound
        private string bluetoothConfirmOnlyFormatString;
        private string bluetoothDisplayPinFormatString;
        private string bluetoothConfirmPinMatchFormatString;
        private Windows.UI.Xaml.Controls.Button inProgressPairButton;
        Windows.UI.Xaml.Controls.Primitives.FlyoutBase savedPairButtonFlyout;

        private bool needsCortanaConsent = false;
        private bool cortanaConsentRequestedFromSwitch = false;

        private SettingsViewModel ViewModel
        {
            get { return DataContext as SettingsViewModel; }
        }

        public SettingsPage()
        {
            InitializeComponent();

            visibleContent = BasicPreferencesGrid;

            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            //this.DataContext = LanguageManager.GetInstance();

            this.Loaded += async (sender, e) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    //SetupLanguages();
                    //screensaverToggleSwitch.IsOn = Screensaver.IsScreensaverEnabled;
                });
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Initialize();

            SettingPreferences.IsChecked = true;
        }


        //private void SettingsChoice_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    var item = e.ClickedItem as FrameworkElement;
        //    if (item == null)
        //    {
        //        return;
        //    }

        //    // Language, Network, or Bluetooth settings etc.
        //    SwitchToSelectedSettingsAsync(item.Name);
        //}


        private void Settings_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton target = sender as RadioButton;
            if (null == target) return;

            SwitchToSelectedSettingsAsync(target.Name);
        }

        private void SwitchToSelectedSettingsAsync(string itemName)
        {
            switch (itemName)
            {
                //case "PreferencesListViewItem":
                case "SettingPreferences":
                    if (BasicPreferencesGrid.Visibility == Visibility.Collapsed)
                    {
                        visibleContent.Visibility = Visibility.Collapsed;
                        BasicPreferencesGrid.Visibility = Visibility.Visible;
                        visibleContent = BasicPreferencesGrid;
                    }
                    break;
                case "SettingNetwork":
                //case "NetworkListViewItem":
                    if (NetworkGrid.Visibility == Visibility.Collapsed)
                    {
                        SettingNetworkControl.SetupNetwork();
                        visibleContent.Visibility = Visibility.Collapsed;
                        NetworkGrid.Visibility = Visibility.Visible;
                        visibleContent = NetworkGrid;
                    }
                    break;
                case "SettingBluetooth":
                //case "BluetoothListViewItem":
                    if (BluetoothGrid.Visibility == Visibility.Collapsed)
                    {
                        SettingBluetoothControl.SetupBluetooth();
                        visibleContent.Visibility = Visibility.Collapsed;
                        BluetoothGrid.Visibility = Visibility.Visible;
                        visibleContent = BluetoothGrid;
                    }
                    break;
                case "SettingZigbee":
                //case "ZigbeeListViewItem":
                    if (ZigbeeGrid.Visibility == Visibility.Collapsed)
                    {
                        SettingZigbeeControl.SetupZigbee();
                        visibleContent.Visibility = Visibility.Collapsed;
                        ZigbeeGrid.Visibility = Visibility.Visible;
                        visibleContent = ZigbeeGrid;
                    }
                    break;
                case "SettingZWave":
                    if (ZWaveGrid.Visibility == Visibility.Collapsed)
                    {
                        SettingZWaveControl.SetupZWave();
                        visibleContent.Visibility = Visibility.Collapsed;
                        ZWaveGrid.Visibility = Visibility.Visible;
                        visibleContent = ZWaveGrid;
                    }
                    break;
            }
        }

        
    }
}
