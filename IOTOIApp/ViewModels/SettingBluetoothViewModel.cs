using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;
using Windows.Foundation;
using Windows.UI.Core;

namespace IOTOIApp.ViewModels
{
    public class SettingBluetoothViewModel : ViewModelBase
    {
        ObservableCollection<BluetoothDeviceInformationDisplay> _bluetoothDevices;
        public ObservableCollection<BluetoothDeviceInformationDisplay> BluetoothDevices
        {
            get { return _bluetoothDevices; }
            private set { Set(ref _bluetoothDevices, value); }
        }

        bool _onBluetooth;
        public bool OnBluetooth
        {
            get { return _onBluetooth; }
            set { Set(ref _onBluetooth, value); }
        }

        string _confirmationText;
        public string ConfirmationText
        {
            get { return _confirmationText; }
            private set { Set(ref _confirmationText, value); }
        }

        bool _yesButtonVisibility;
        public bool YesButtonVisibility
        {
            get { return _yesButtonVisibility; }
            private set { Set(ref _yesButtonVisibility, value); }
        }

        bool _noButtonVisibility;
        public bool NoButtonVisibility
        {
            get { return _noButtonVisibility; }
            private set { Set(ref _noButtonVisibility, value); }
        }

        bool _stackLoadingVisibility;
        public bool StackLoadingVisibility
        {
            get { return _stackLoadingVisibility; }
            private set { Set(ref _stackLoadingVisibility, value); }
        }

        public ICommand YesButtonClickedCommand { get; private set; }
        public ICommand NoButtonClickedCommand { get; private set; }

        // Device watcher
        DeviceWatcher deviceWatcher = null;
        TypedEventHandler<DeviceWatcher, DeviceInformation> handlerAdded = null;
        TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> handlerUpdated = null;
        TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> handlerRemoved = null;
        TypedEventHandler<DeviceWatcher, Object> handlerEnumCompleted = null;
        TypedEventHandler<DeviceWatcher, Object> handlerStopped = null;

        Windows.Devices.Enumeration.DevicePairingRequestedEventArgs pairingRequestedHandlerArgs;
        Windows.Foundation.Deferral deferral;
        RfcommServiceProvider provider = null; // To be used for inbound

        public enum MessageType { YesNoMessage, OKMessage, InformationalMessage };

        public string BluetoothConfirmOnlyFormatString { get; set; }
        public string BluetoothDisplayPinFormatString { get; set; }
        public string BluetoothConfirmPinMatchFormatString { get; set; }

        string _bluetoothPairButtonContent;
        public string BluetoothPairButtonContent { get { return _bluetoothPairButtonContent; } set { Set(ref _bluetoothPairButtonContent, value); } }

        string _bluetoothEnterPINText;
        public string BluetoothEnterPINText { get { return _bluetoothEnterPINText; } set { Set(ref _bluetoothEnterPINText, value); } }

        string _bluetoothUnpairButtonContent;
        public string BluetoothUnpairButtonContent { get { return _bluetoothUnpairButtonContent; } set { Set(ref _bluetoothUnpairButtonContent, value); } }

        public Windows.UI.Xaml.Controls.Button InProgressPairButton;
        public Windows.UI.Xaml.Controls.Primitives.FlyoutBase SavedPairButtonFlyout;


        public SettingBluetoothViewModel()
        {
            BluetoothDevices = new ObservableCollection<BluetoothDeviceInformationDisplay>();

            YesButtonClickedCommand = new RelayCommand(YesButtonClicked);
            NoButtonClickedCommand = new RelayCommand(NoButtonClicked);

            StackLoadingVisibility = true;
        }




        void ClearConfirmationPanel()
        {
            ConfirmationText = "";
            YesButtonVisibility = false;
            NoButtonVisibility = false;
        }


        /// <summary>
        /// Turn on Bluetooth Radio and list available Bluetooth Devices
        /// </summary>
        public async void TurnOnRadio()
        {
            StackLoadingVisibility = true;

            // Display a message
            ResourceLoader loader = ResourceLoader.GetForCurrentView();
            string confirmationMessage = loader.GetString("BluetoothOn/Text");

            DisplayMessagePanelAsync(confirmationMessage, MessageType.InformationalMessage);

            //Restart the Discoverability
            //Check: App.IsBluetoothDiscoverable = false;

            await ToggleBluetoothAsync(true);
            RegisterForInboundPairingRequests();
        }


        public async void TurnOffBluetooth()
        {
            // Clear any devices in the list
            BluetoothDevices.Clear();
            // Stop the watcher
            //Check StopWatcher();

            // Display a message
            ResourceLoader loader = ResourceLoader.GetForCurrentView();
            string confirmationMessage = loader.GetString("BluetoothOff/Text");
            DisplayMessagePanelAsync(confirmationMessage, MessageType.InformationalMessage);

            StackLoadingVisibility = false;

            await ToggleBluetoothAsync(false);
        }


        public async Task<bool> IsBluetoothEnabledAsync()
        {
            var radios = await Radio.GetRadiosAsync();
            var bluetoothRadio = radios.FirstOrDefault(radio => radio.Kind == RadioKind.Bluetooth);
            return bluetoothRadio != null && bluetoothRadio.State == RadioState.On;
        }


        async Task ToggleBluetoothAsync(bool bluetoothState)
        {
            try
            {
                var access = await Radio.RequestAccessAsync();
                if (access != RadioAccessStatus.Allowed)
                {
                    return;
                }
                BluetoothAdapter adapter = await BluetoothAdapter.GetDefaultAsync();
                if (null != adapter)
                {
                    var btRadio = await adapter.GetRadioAsync();
                    if (bluetoothState)
                    {
                        await btRadio.SetStateAsync(RadioState.On);
                    }
                    else
                    {
                        await btRadio.SetStateAsync(RadioState.Off);
                    }
                }
                else
                {
                    if (bluetoothState)
                        NoDeviceFound();
                }

            }
            catch (Exception e)
            {
                NoDeviceFound(e.Message);
            }
        }


        void NoDeviceFound(string message = "")
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView();

            string formatString = loader.GetString("BluetoothNoDeviceAvailable/Text");
            string confirmationMessage = formatString + message;
            DisplayMessagePanelAsync(confirmationMessage, MessageType.InformationalMessage);

            StackLoadingVisibility = false;
        }


        async void RegisterForInboundPairingRequests()
        {
            // Make the system discoverable for Bluetooth
            await MakeDiscoverable();

            // If the attempt to make the system discoverable failed then likely there is no Bluetooth device present
            // so leave the diagnositic message put out by the call to MakeDiscoverable()
            if (App.IsBluetoothDiscoverable)
            {
                string formatString;
                string confirmationMessage;

                // Get state of ceremony checkboxes
                DevicePairingKinds ceremoniesSelected = GetSelectedCeremonies();
                int iCurrentSelectedCeremonies = (int)ceremoniesSelected;

                // Find out if we changed the ceremonies we orginally registered with - if we have registered before these will be saved
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                Object supportedPairingKinds = localSettings.Values["supportedPairingKinds"];
                int iSavedSelectedCeremonies = -1; // Deliberate impossible value
                if (supportedPairingKinds != null)
                {
                    iSavedSelectedCeremonies = (int)supportedPairingKinds;
                }

                ResourceLoader loader = ResourceLoader.GetForCurrentView();

                if (!DeviceInformationPairing.TryRegisterForAllInboundPairingRequests(ceremoniesSelected))
                {
                    confirmationMessage = loader.GetString("BluetoothInboundRegistrationFailed/Text");
                }
                else
                {
                    // Save off the ceremonies we registered with
                    localSettings.Values["supportedPairingKinds"] = iCurrentSelectedCeremonies;
                    formatString = loader.GetString("BluetoothInboundRegistrationSucceeded/Text");
                    confirmationMessage = formatString + ceremoniesSelected.ToString();
                }

                // Clear the current collection
                BluetoothDevices.Clear();
                // Start the watcher
                StartWatcher();
                // Display a message
                confirmationMessage += loader.GetString("BluetoothOn/Text");
                DisplayMessagePanelAsync(confirmationMessage, MessageType.InformationalMessage);
            }
        }

        async System.Threading.Tasks.Task MakeDiscoverable()
        {
            // Make the system discoverable. Don'd repeatedly do this or the StartAdvertising will throw "cannot create a file when that file already exists"
            if (!App.IsBluetoothDiscoverable)
            {
                Guid BluetoothServiceUuid = new Guid("17890000-0068-0069-1532-1992D79BE4D8");
                try
                {
                    provider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.FromUuid(BluetoothServiceUuid));
                    Windows.Networking.Sockets.StreamSocketListener listener = new Windows.Networking.Sockets.StreamSocketListener();
                    listener.ConnectionReceived += OnConnectionReceived;
                    await listener.BindServiceNameAsync(provider.ServiceId.AsString(), Windows.Networking.Sockets.SocketProtectionLevel.PlainSocket);
                    //     SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
                    // Don't bother setting SPD attributes
                    provider.StartAdvertising(listener, true);
                    App.IsBluetoothDiscoverable = true;
                }
                catch (Exception e)
                {
                    ResourceLoader loader = ResourceLoader.GetForCurrentView();
                    string formatString = loader.GetString("BluetoothNoDeviceAvailable/Text");
                    string confirmationMessage = formatString + e.Message;
                    DisplayMessagePanelAsync(confirmationMessage, MessageType.InformationalMessage);
                }
            }
        }


        public DevicePairingKinds GetSelectedCeremonies()
        {
            DevicePairingKinds ceremonySelection = DevicePairingKinds.ConfirmOnly | DevicePairingKinds.DisplayPin | DevicePairingKinds.ProvidePin | DevicePairingKinds.ConfirmPinMatch;
            return ceremonySelection;
        }


        /// <summary>
        /// We have to have a callback handler to handle "ConnectionReceived" but we don't do anything because
        /// the StartAdvertising is just a way to turn on Bluetooth discoverability
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="args"></param>
        void OnConnectionReceived(Windows.Networking.Sockets.StreamSocketListener listener,
                                   Windows.Networking.Sockets.StreamSocketListenerConnectionReceivedEventArgs args)
        {
        }


        /// <summary>
        /// Start the Device Watcher and set callbacks to handle devices appearing and disappearing
        /// </summary>
        public void StartWatcher()
        {
            //ProtocolSelectorInfo protocolSelectorInfo;
            string aqsFilter = @"System.Devices.Aep.ProtocolId:=""{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}"" OR System.Devices.Aep.ProtocolId:=""{bb7bb05e-5972-42b5-94fc-76eaa7084d49}""";  //Bluetooth + BluetoothLE

            // Request the IsPaired property so we can display the paired status in the UI
            string[] requestedProperties = { "System.Devices.Aep.IsPaired" };

            //// Get the device selector chosen by the UI, then 'AND' it with the 'CanPair' property
            //protocolSelectorInfo = (ProtocolSelectorInfo)selectorComboBox.SelectedItem;
            //aqsFilter = protocolSelectorInfo.Selector + " AND System.Devices.Aep.CanPair:=System.StructuredQueryType.Boolean#True";

            deviceWatcher = DeviceInformation.CreateWatcher(
                aqsFilter,
                requestedProperties,
                DeviceInformationKind.AssociationEndpoint
                );

            // Hook up handlers for the watcher events before starting the watcher

            handlerAdded = new TypedEventHandler<DeviceWatcher, DeviceInformation>(async (watcher, deviceInfo) =>
            {
                // Since we have the collection databound to a UI element, we need to update the collection on the UI thread.
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    BluetoothDevices.Add(new BluetoothDeviceInformationDisplay(deviceInfo));
                });
            });
            deviceWatcher.Added += handlerAdded;

            handlerUpdated = new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
            {
                // Since we have the collection databound to a UI element, we need to update the collection on the UI thread.
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    // Find the corresponding updated DeviceInformation in the collection and pass the update object
                    // to the Update method of the existing DeviceInformation. This automatically updates the object
                    // for us.
                    foreach (BluetoothDeviceInformationDisplay deviceInfoDisp in BluetoothDevices)
                    {
                        if (deviceInfoDisp.Id == deviceInfoUpdate.Id)
                        {
                            deviceInfoDisp.Update(deviceInfoUpdate);
                            break;
                        }
                    }
                });
            });
            deviceWatcher.Updated += handlerUpdated;

            handlerRemoved = new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
            {
                // Since we have the collection databound to a UI element, we need to update the collection on the UI thread.
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    // Find the corresponding DeviceInformation in the collection and remove it
                    foreach (BluetoothDeviceInformationDisplay deviceInfoDisp in BluetoothDevices)
                    {
                        if (deviceInfoDisp.Id == deviceInfoUpdate.Id)
                        {
                            BluetoothDevices.Remove(deviceInfoDisp);
                            break;
                        }
                    }
                });
            });
            deviceWatcher.Removed += handlerRemoved;

            handlerEnumCompleted = new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    // Finished enumerating
                    StackLoadingVisibility = false;

                    ResourceLoader loader = ResourceLoader.GetForCurrentView();

                    var index = ConfirmationText.IndexOf(loader.GetString("BluetoothOn/Text"));
                    if (index != -1)
                    {
                        DisplayMessagePanelAsync(ConfirmationText.Substring(0, index), MessageType.InformationalMessage, true);
                    }

                });
            });
            deviceWatcher.EnumerationCompleted += handlerEnumCompleted;

            handlerStopped = new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    // Device watcher stopped
                    StackLoadingVisibility = false;
                });
            });
            deviceWatcher.Stopped += handlerStopped;

            // Start the Device Watcher
            deviceWatcher.Start();
            StackLoadingVisibility = true;
        }

        /// <summary>
        /// Stop the Device Watcher
        /// </summary>
        public void StopWatcher()
        {
            if (null != deviceWatcher)
            {
                // First unhook all event handlers except the stopped handler. This ensures our
                // event handlers don't get called after stop, as stop won't block for any "in flight" 
                // event handler calls.  We leave the stopped handler as it's guaranteed to only be called
                // once and we'll use it to know when the query is completely stopped. 
                deviceWatcher.Added -= handlerAdded;
                deviceWatcher.Updated -= handlerUpdated;
                deviceWatcher.Removed -= handlerRemoved;
                deviceWatcher.EnumerationCompleted -= handlerEnumCompleted;

                if (DeviceWatcherStatus.Started == deviceWatcher.Status ||
                    DeviceWatcherStatus.EnumerationCompleted == deviceWatcher.Status)
                {
                    deviceWatcher.Stop();
                    StackLoadingVisibility = false;
                }
            }
        }


        public void AcceptPairingWithPIN(string PIN)
        {
            if (pairingRequestedHandlerArgs != null)
            {
                pairingRequestedHandlerArgs.Accept(PIN);
                pairingRequestedHandlerArgs = null;
            }
            // Complete the deferral here
            CompleteDeferral();
        }


        void CompleteDeferral()
        {
            // Complete the deferral
            if (deferral != null)
            {
                deferral.Complete();
                deferral = null;
            }
        }

        /// <summary>
        /// Accept the pairing and complete the deferral
        /// </summary>
        void AcceptPairing()
        {
            if (pairingRequestedHandlerArgs != null)
            {
                pairingRequestedHandlerArgs.Accept();
                pairingRequestedHandlerArgs = null;
            }
            // Complete deferral
            CompleteDeferral();
        }


        /// <summary>
        /// Called when custom pairing is initiated so that we can handle the custom ceremony
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void PairingRequestedHandler(
            DeviceInformationCustomPairing sender,
            DevicePairingRequestedEventArgs args)
        {
            //Null Check
            if (null == args)
                return;

            // Save the args for use in ProvidePin case
            pairingRequestedHandlerArgs = args;

            // Save the deferral away and complete it where necessary.
            if (args.PairingKind != DevicePairingKinds.DisplayPin)
            {
                deferral = args.GetDeferral();
            }

            string confirmationMessage;

            switch (args.PairingKind)
            {
                case DevicePairingKinds.ConfirmOnly:
                    // Windows itself will pop the confirmation dialog as part of "consent" if this is running on Desktop or Mobile
                    // If this is an App for Athens where there is no Windows Consent UX, you may want to provide your own confirmation.
                    {
                        confirmationMessage = BluetoothConfirmOnlyFormatString;
                        confirmationMessage += ((null != args.DeviceInformation) ? args.DeviceInformation.Name : string.Empty);
                        confirmationMessage += ((null != args.DeviceInformation) ? " (" + args.DeviceInformation.Id + ")" : " ()");

                        DisplayMessagePanelAsync(confirmationMessage, MessageType.InformationalMessage);
                        // Accept the pairing which also completes the deferral
                        AcceptPairing();
                    }
                    break;

                case DevicePairingKinds.DisplayPin:
                    // We just show the PIN on this side. The ceremony is actually completed when the user enters the PIN
                    // on the target device
                    {
                        confirmationMessage = BluetoothDisplayPinFormatString + args.Pin;
                        DisplayMessagePanelAsync(confirmationMessage, MessageType.OKMessage);
                    }
                    break;

                case DevicePairingKinds.ProvidePin:
                    // A PIN may be shown on the target device and the user needs to enter the matching PIN on 
                    // this Windows device.
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                    {
                        // PIN Entry
                        InProgressPairButton.Flyout = SavedPairButtonFlyout;
                        InProgressPairButton.Flyout.ShowAt(InProgressPairButton);
                    });
                    break;

                case DevicePairingKinds.ConfirmPinMatch:
                    // We show the PIN here and the user responds with whether the PIN matches what they see
                    // on the target device. Response comes back and we set it on the PinComparePairingRequestedData
                    // then complete the deferral.
                    {
                        confirmationMessage = BluetoothConfirmPinMatchFormatString + args.Pin;
                        DisplayMessagePanelAsync(confirmationMessage, MessageType.YesNoMessage);
                    }
                    break;
            }
        }


        void YesButtonClicked()
        {
            // Accept the pairing
            AcceptPairing();

            ResourceLoader loader = ResourceLoader.GetForCurrentView();
            DisplayMessagePanelAsync(loader.GetString("BluetoothPairingRequestProgress/Text"), MessageType.InformationalMessage);

        }


        /// <summary>
        /// The No button on the DisplayConfirmationPanelAndComplete - completes the deferral and clears the message panel
        /// </summary>
        void NoButtonClicked()
        {
            //Complete the deferral
            CompleteDeferral();
            // Clear the confirmation message
            ClearConfirmationPanel();
        }


        /// <summary>
        /// This is really just a replacement for MessageDialog, which you can't use on Athens
        /// </summary>
        /// <param name="confirmationMessage"></param>
        /// <param name="messageType"></param>
        public async void DisplayMessagePanelAsync(string confirmationMessage, MessageType messageType, bool force = false)
        {
            // Use UI thread
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (!force && !OnBluetooth)
                {
                    ClearConfirmationPanel();
                }

                ConfirmationText = confirmationMessage;
                if (messageType == MessageType.OKMessage)
                {
                    YesButtonVisibility = true;
                    NoButtonVisibility = false;
                }
                else if (messageType == MessageType.InformationalMessage)
                {
                    // Just make the buttons invisible
                    YesButtonVisibility = false;
                    NoButtonVisibility = false;
                }
                else
                {
                    YesButtonVisibility = true;
                    NoButtonVisibility = true;
                }
            });
        }


    }
}
