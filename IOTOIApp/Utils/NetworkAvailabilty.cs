using IOTOIApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace IOTOIApp.Utils
{
    public class NetworkAvailabilty
    {
        private static NetworkAvailabilty _networkAvailabilty;
        public static NetworkAvailabilty Instance
        {
            get { return _networkAvailabilty ?? (_networkAvailabilty = new NetworkAvailabilty()); }
            set { _networkAvailabilty = value; }
        }

        private bool _isNetworkAvailable;
        public event Action<bool> OnNetworkAvailabilityChange = delegate { };

        public bool IsNetworkAvailable
        {
            get
            {
                return _isNetworkAvailable;
            }
            protected set
            {
                if (_isNetworkAvailable == value) return;
                _isNetworkAvailable = value;
                OnNetworkAvailabilityChange(value);
            }
        }

        private void CheckInternetAccess()
        {
            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            IsNetworkAvailable = (connectionProfile != null &&
                                 connectionProfile.GetNetworkConnectivityLevel() ==
                                 NetworkConnectivityLevel.InternetAccess);
            Debug.WriteLine("has network changed: " + IsNetworkAvailable);
        }

        private void NetworkInformationOnNetworkStatusChanged(object sender)
        {
            CheckInternetAccess();
            Debug.WriteLine("network status changed");
        }

        private NetworkAvailabilty()
        {
            NetworkInformation.NetworkStatusChanged += NetworkInformationOnNetworkStatusChanged;
            CheckInternetAccess();
        }
    }

}
