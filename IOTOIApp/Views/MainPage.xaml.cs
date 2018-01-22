using IOTOIApp.ViewModels;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Diagnostics;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace IOTOIApp.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
        }

        public MainPage()
        {
            InitializeComponent();

            LoggingChannel lc = new LoggingChannel("my provider", null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));
            lc.LogMessage("I made a message!");

        }
    }
}
