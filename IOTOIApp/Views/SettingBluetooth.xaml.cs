using IOTOIApp.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class SettingBluetooth : Page
    {
        SettingBluetoothViewModel ViewModel
        {
            get { return DataContext as SettingBluetoothViewModel; }
        }


        public SettingBluetooth()
        {
            this.InitializeComponent();

            this.Loaded += SettingBluetooth_Loaded;
        }


        private void SettingBluetooth_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= SettingBluetooth_Loaded;


            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            ViewModel.BluetoothConfirmOnlyFormatString = loader.GetString("BluetoothConfirmOnly/Text");
            ViewModel.BluetoothDisplayPinFormatString = loader.GetString("BluetoothDisplayPin/Text");
            ViewModel.BluetoothConfirmPinMatchFormatString = loader.GetString("BluetoothConfirmPinMatch/Text");

            ViewModel.BluetoothPairButtonContent = loader.GetString("BluetoothPairButtonContent/Text");
            ViewModel.BluetoothEnterPINText = loader.GetString("BluetoothEnterPINText/Text");
            ViewModel.BluetoothUnpairButtonContent = loader.GetString("BluetoothUnpairButtonContent/Text");

            SwitchBluetooth.Checked += SwitchBluetooth_Checked;
            SwitchBluetooth.Unchecked += SwitchBluetooth_Unchecked;
        }


        private void SwitchBluetooth_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.OnBluetooth = true;
            ViewModel.TurnOnRadio();
        }


        private void SwitchBluetooth_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.OnBluetooth = false;
            ViewModel.TurnOffBluetooth();
        }


        public async void SetupBluetooth()
        {
            if (await ViewModel.IsBluetoothEnabledAsync())
            {
                SwitchBluetooth.IsChecked = true;
            }
            else
            {
                ViewModel.TurnOffBluetooth();
            }
        }


        /// <summary>
        /// User wants to use custom pairing with the selected ceremony types and Default protection level
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PairButton_Click(object sender, RoutedEventArgs e)
        {
            // Use the pair button on the bluetoothDeviceListView.SelectedItem to get the data context
            BluetoothDeviceInformationDisplay deviceInfoDisp = ((Button)sender).DataContext as BluetoothDeviceInformationDisplay;

            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            string formatString = loader.GetString("BluetoothAttemptingToPair/Text");
            string confirmationMessage = formatString + deviceInfoDisp.Name + " (" + deviceInfoDisp.Id + ")";
            ViewModel.DisplayMessagePanelAsync(confirmationMessage, SettingBluetoothViewModel.MessageType.InformationalMessage);

            // Save the pair button
            Button pairButton = sender as Button;
            ViewModel.InProgressPairButton = pairButton;

            // Save the flyout and set to null so it doesn't pop up unless we want it
            ViewModel.SavedPairButtonFlyout = pairButton.Flyout;
            ViewModel.InProgressPairButton.Flyout = null;

            // Disable the pair button until we are done
            pairButton.IsEnabled = false;

            // Get ceremony type and protection level selections
            DevicePairingKinds ceremoniesSelected = ViewModel.GetSelectedCeremonies();
            // Get protection level
            DevicePairingProtectionLevel protectionLevel = DevicePairingProtectionLevel.Default;

            // Specify custom pairing with all ceremony types and protection level EncryptionAndAuthentication
            DeviceInformationCustomPairing customPairing = deviceInfoDisp.DeviceInformation.Pairing.Custom;

            customPairing.PairingRequested += ViewModel.PairingRequestedHandler;
            DevicePairingResult result = await customPairing.PairAsync(ceremoniesSelected, protectionLevel);

            if (result.Status == DevicePairingResultStatus.Paired)
            {
                formatString = loader.GetString("BluetoothPairingSuccess/Text");
                confirmationMessage = formatString + deviceInfoDisp.Name + " (" + deviceInfoDisp.Id + ")";
            }
            else
            {
                formatString = loader.GetString("BluetoothPairingFailure/Text");
                confirmationMessage = formatString + deviceInfoDisp.Name + " (" + deviceInfoDisp.Id + ")"; // result.Status.ToString()
            }
            // Display the result of the pairing attempt
            ViewModel.DisplayMessagePanelAsync(confirmationMessage, SettingBluetoothViewModel.MessageType.InformationalMessage);

            // If the watcher toggle is on, clear any devices in the list and stop and restart the watcher to ensure state is reflected in list
            if (SwitchBluetooth.IsChecked.Value)
            {
                ViewModel.BluetoothDevices.Clear();
                ViewModel.StopWatcher();
                ViewModel.StartWatcher();
            }
            else
            {
                // If the watcher is off this is an inbound request so just clear the list
                ViewModel.BluetoothDevices.Clear();
            }

            // Re-enable the pair button
            ViewModel.InProgressPairButton = null;
            pairButton.IsEnabled = true;
        }


        /// <summary>
        /// User wants to unpair from the selected device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UnpairButton_Click(object sender, RoutedEventArgs e)
        {
            // Use the unpair button on the bluetoothDeviceListView.SelectedItem to get the data context
            BluetoothDeviceInformationDisplay deviceInfoDisp = ((Button)sender).DataContext as BluetoothDeviceInformationDisplay;
            string formatString;
            string confirmationMessage;

            Button unpairButton = sender as Button;
            // Disable the unpair button until we are done
            unpairButton.IsEnabled = false;

            DeviceUnpairingResult unpairingResult = await deviceInfoDisp.DeviceInformation.Pairing.UnpairAsync();

            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            if (unpairingResult.Status == DeviceUnpairingResultStatus.Unpaired)
            {
                // Device is unpaired
                formatString = loader.GetString("BluetoothUnpairingSuccess/Text");
                confirmationMessage = formatString + deviceInfoDisp.Name + " (" + deviceInfoDisp.Id + ")";
            }
            else
            {
                formatString = loader.GetString("BluetoothUnpairingFailure/Text");
                confirmationMessage = formatString + deviceInfoDisp.Name + " (" + deviceInfoDisp.Id + ")"; // unpairingResult.Status.ToString()
            }
            // Display the result of the pairing attempt
            ViewModel.DisplayMessagePanelAsync(confirmationMessage, SettingBluetoothViewModel.MessageType.InformationalMessage);

            // If the watcher toggle is on, clear any devices in the list and stop and restart the watcher to ensure state is reflected in list
            if (SwitchBluetooth.IsChecked.Value)
            {
                ViewModel.BluetoothDevices.Clear();
                ViewModel.StopWatcher();
                ViewModel.StartWatcher();
            }
            else
            {
                // If the watcher is off this is an inbound request so just clear the list
                ViewModel.BluetoothDevices.Clear();
            }

            // Re-enable the unpair button
            unpairButton.IsEnabled = true;
        }


        /// <summary>
        /// User has entered a PIN and pressed <Return> in the PIN entry flyout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PinEntryTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                //  Close the flyout and save the PIN the user entered
                TextBox bluetoothPINTextBox = sender as TextBox;
                string pairingPIN = bluetoothPINTextBox.Text;
                if (pairingPIN != "")
                {
                    // Hide the flyout
                    ViewModel.InProgressPairButton.Flyout.Hide();
                    ViewModel.InProgressPairButton.Flyout = null;
                    // Use the PIN to accept the pairing
                    ViewModel.AcceptPairingWithPIN(pairingPIN);
                }
            }
        }
    }
}
