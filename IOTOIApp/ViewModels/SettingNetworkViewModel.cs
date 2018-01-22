using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Windows.Devices.WiFi;
using Windows.Security.Credentials;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;

using IOTOIApp.Utils;
using System.Collections.ObjectModel;
using Windows.System;
using Windows.UI.Xaml.Input;

namespace IOTOIApp.ViewModels
{
    public class SettingNetworkViewModel : ViewModelBase
    {
        private static NetworkPresenter networkPresenter = new NetworkPresenter();

        private Visibility _noneFoundVisibility = Visibility.Collapsed;
        public Visibility NoneFoundVisibility
        {
            get { return _noneFoundVisibility; }
            set { Set(ref _noneFoundVisibility, value); }
        }

        private Visibility _directConnectionVisibility;
        public Visibility DirectConnectionVisibility
        {
            get { return _directConnectionVisibility; }
            set { Set(ref _directConnectionVisibility, value); }
        }

        private ObservableCollection<WiFiAvailableNetwork> _wifiListSources;
        public ObservableCollection<WiFiAvailableNetwork> WifiListSources
        {
            get { return _wifiListSources; }
            set { Set(ref _wifiListSources, value); }
        }

        private object _wifiSelected;
        public object WifiSelected
        {
            get { return _wifiSelected; }
            set { Set(ref _wifiSelected, value); }
        }

        private WiFiAvailableNetwork _connectedNetwork;
        public WiFiAvailableNetwork ConnectedNetwork
        {
            get { return _connectedNetwork; }
            set { Set(ref _connectedNetwork, value); }
        }

        private string _noWifiFoundText;
        public string NoWifiFoundText
        {
            get { return _noWifiFoundText; }
            set { Set(ref _noWifiFoundText, value); }
        }

        private Visibility _noWifiFoundVisibility = Visibility.Collapsed;
        public Visibility NoWifiFoundVisibility
        {
            get { return _noWifiFoundVisibility; }
            set { Set(ref _noWifiFoundVisibility, value); }
        }

        private Visibility _wifiListVisibility = Visibility.Visible;
        public Visibility WifiListVisibility
        {
            get { return _wifiListVisibility; }
            set { Set(ref _wifiListVisibility, value); }
        }

        private string _wifiPassword;
        public string WifiPassword
        {
            get { return _wifiPassword; }
            set { Set(ref _wifiPassword, value); }
        }

        private Boolean _refreshEnabled = true;
        public Boolean RefreshEnabled
        {
            get { return _refreshEnabled; }
            set { Set(ref _refreshEnabled, value); }
        }

        private Boolean _automaticallyChecked = true;
        public Boolean AutomaticallyChecked
        {
            get { return _automaticallyChecked; }
            set { Set(ref _automaticallyChecked, value); }
        }

        private Boolean _disconnectButtonEnabled = true;
        public Boolean DisconnectButtonEnabled
        {
            get { return _disconnectButtonEnabled; }
            set { Set(ref _disconnectButtonEnabled, value); }
        }
        
        public ListView WifiListView;

        public ICommand ConnectButtonClickedCommand { get; private set; }
        public ICommand NextButtonClickedCommand { get; private set; }
        public ICommand RefreshWifiListCommand { get; private set; }
        public ICommand DisconnectWifiCommand { get; private set; }
        public ICommand CancelButtonClickedCommand { get; private set; }
        public ICommand PasswordEnterCommand { get; private set; }
        

        public SettingNetworkViewModel()
        {
            ConnectButtonClickedCommand = new RelayCommand<WiFiAvailableNetwork>(ConnectButtonClicked);
            NextButtonClickedCommand = new RelayCommand(NextButtonClicked);
            RefreshWifiListCommand = new RelayCommand(RefreshWifiList);
            DisconnectWifiCommand = new RelayCommand(DisconnectWifi);
            CancelButtonClickedCommand = new RelayCommand(CancelButtonClicked);
            PasswordEnterCommand = new RelayCommand<KeyRoutedEventArgs>(NextButtonClicked);
        }

        public async void SetupNetwork()
        {
            try
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                    SetupEthernet();
                    await SetupWifi();
                });

            }
            catch (Exception e)
            {
                Debug.WriteLine("SetupNetwork Exception : " + e.Message);
            }
        }

        private void SetupEthernet()
        {
            var ethernetProfile = NetworkPresenter.GetDirectConnectionName();

            if (ethernetProfile == null)
            {
                NoneFoundVisibility = Visibility.Visible;
                DirectConnectionVisibility = Visibility.Collapsed;
            }
            else
            {
                NoneFoundVisibility = Visibility.Collapsed;
                DirectConnectionVisibility = Visibility.Visible;
            }
        }

        private async Task SetupWifi()
        {
            Debug.WriteLine("SetupWifi start");
            if (await networkPresenter.WifiIsAvailable())
            {
                IList<WiFiAvailableNetwork> networks;
                try
                {
                    networks = await networkPresenter.GetAvailableNetworks();
                    var emptys = networks.Where(w => string.IsNullOrEmpty(w.Ssid.Trim())).ToList();
                    foreach (WiFiAvailableNetwork wn in emptys)
                    {
                        networks.Remove(wn);
                    }
                    networks = networks.OrderBy(w => w.Ssid).ToList();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(String.Format("Error scanning: 0x{0:X}: {1}", e.HResult, e.Message));
                    DisconnectButtonEnabled = true;
                    NoWifiFoundText = e.Message;
                    NoWifiFoundVisibility = Visibility.Visible;
                    return;
                }
                Debug.WriteLine("SetupWifi networks :: " + networks.Count);
                if (networks.Count > 0)
                {
                    ConnectedNetwork = networkPresenter.GetCurrentWifiNetwork();
                    Debug.WriteLine("SetupWifi ConnectedNetwork :: " + ConnectedNetwork);
                    if (ConnectedNetwork != null)
                    {
                        networks.Remove(ConnectedNetwork);
                        networks.Insert(0, ConnectedNetwork);
                        WifiListSources = new ObservableCollection<WiFiAvailableNetwork>(networks);
                        Debug.WriteLine("SetupWifi UpdateLayout!!!!!!!!!! :: ");
                        SwitchToItemState(ConnectedNetwork, "WifiConnectedState", true);
                    }
                    else
                    {
                        WifiListSources = new ObservableCollection<WiFiAvailableNetwork>(networks);
                    }

                    DisconnectButtonEnabled = true;
                    NoWifiFoundVisibility = Visibility.Collapsed;
                    WifiListVisibility = Visibility.Visible;

                    return;
                }
            }

            DisconnectButtonEnabled = true;
            NoWifiFoundVisibility = Visibility.Visible;
            WifiListVisibility = Visibility.Collapsed;
            Debug.WriteLine("SetupWifi end");
        }

        WiFiAvailableNetwork PrevSel = null;
        public void WifiSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null != PrevSel)
            {
                if (ConnectedNetwork == PrevSel)
                {
                    SwitchToItemState(PrevSel, "WifiConnectedState", true);
                }
                else
                {
                    SwitchToItemState(PrevSel, "InitialState", true);
                }
            }


            foreach (var item in e.AddedItems)
            {
                AutomaticallyChecked = true;
                if (ConnectedNetwork == item)
                {
                    SwitchToItemState(ConnectedNetwork, "WifiConnectedMoreOptions", true);
                }
                else
                {
                    SwitchToItemState(item, "WifiConnectState", true);
                }

                PrevSel = item as WiFiAvailableNetwork;
            }
        }

        private void ConnectButtonClicked(WiFiAvailableNetwork network)
        {
            if (NetworkPresenter.IsNetworkOpen(network))
            {
                ConnectToWifi(network, null, Window.Current.Dispatcher);
            }
            else
            {
                WifiPassword = "";
                SwitchToItemState(network, "WifiPasswordState", false);
            }
        }

        private void NextButtonClicked(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && e.KeyStatus.RepeatCount == 0)
            {
                NextButtonClicked();
            }
        }

        private void NextButtonClicked()
        {
            PasswordCredential credential;

            if (string.IsNullOrEmpty(WifiPassword))
            {
                credential = null;
            }
            else
            {
                credential = new PasswordCredential()
                {
                    Password = WifiPassword
                };
            }
            ConnectToWifi(WifiSelected as WiFiAvailableNetwork, credential, Window.Current.Dispatcher);
        }

        private async void ConnectToWifi(WiFiAvailableNetwork network, PasswordCredential credential, CoreDispatcher dispatcher)
        {
            var didConnect = credential == null ?
                networkPresenter.ConnectToNetwork(network, AutomaticallyChecked) :
                networkPresenter.ConnectToNetworkWithPassword(network, AutomaticallyChecked, credential);

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SwitchToItemState(network, "WifiConnectingState", false);
            });

            //string nextState = (await didConnect) ? "WifiConnectedState" : "InitialState";

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SwitchToItemState(ConnectedNetwork, "InitialState", false);
            });

            if (await didConnect)
            {
                ConnectedNetwork = network;
                WifiListSources.Remove(ConnectedNetwork);
                WifiListSources.Insert(0, ConnectedNetwork);
                SwitchToItemState(ConnectedNetwork, "WifiConnectedState", true);

            }
            else
            {
                ConnectedNetwork = null;
                WifiSelected = null;
            }
        }

        private async void DisconnectWifi()
        {
            DisconnectButtonEnabled = false;
            WiFiAvailableNetwork network = WifiSelected as WiFiAvailableNetwork;

            if (network == ConnectedNetwork)
            {
                networkPresenter.DisconnectNetwork(network);
                System.Threading.Tasks.Task.Delay(3000).Wait();
            }
            SwitchToItemState(network, "InitialState", true);

            SetupEthernet();
            await SetupWifi();
        }

        private void SwitchToItemState(object dataContext, string template, bool forceUpdate)
        {
            if (forceUpdate)
            {
                WifiListView.UpdateLayout();
            }

            var item = WifiListView.ContainerFromItem(dataContext) as ListViewItem;
            if (item != null)
            {
                item.ContentTemplate = WifiListViewDataTemplateSelector.DataTemplates[template];
            }
        }

        private void CancelButtonClicked()
        {
            WiFiAvailableNetwork network = WifiSelected as WiFiAvailableNetwork;

            WifiListSources.Remove(network);
            WifiListSources.Insert(0, network);

            //SwitchToItemState(WifiSelected as WiFiAvailableNetwork, "InitialState", false);
        }

        private async void RefreshWifiList()
        {
            RefreshEnabled = false;
            SetupEthernet();
            await SetupWifi();
            RefreshEnabled = true;
        }

        private ListViewItem ContainerFromItem(SelectionChangedEventArgs e)
        {
            return ((ListViewItem)
                ((ListView)e.OriginalSource)
                .ContainerFromItem(e.AddedItems));
        }

    }
}
