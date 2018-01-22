using System;
using IOTOIApp.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IOTOIApp.Views
{
    public sealed partial class AppListPage : Page
    {
        private AppListViewModel ViewModel
        {
            get { return DataContext as AppListViewModel; }
        }

        public AppListPage()
        {
            InitializeComponent();
        }
    }
}
