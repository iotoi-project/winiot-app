using IOTOIApp.Services;
using IOTOIApp.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IOTOIApp.Views
{
    public sealed partial class HeaderPage : Page
    {
        private HeaderViewModel ViewModel
        {
            get { return DataContext as HeaderViewModel; }
        }

        public HeaderPage()
        {
            InitializeComponent();
        }
    }
}
