using IOTOIApp.ViewModels.CCTV;
using System;
using Windows.UI.Xaml.Controls;

namespace IOTOIApp.Views.CCTV
{
    public sealed partial class CCTVMainPage : Page
    {
        private CCTVMainViewModel ViewModel
        {
            get { return DataContext as CCTVMainViewModel; }
        }

        public CCTVMainPage()
        {
            InitializeComponent();

            ViewModel.StartImageStream();
        }
    }
}
