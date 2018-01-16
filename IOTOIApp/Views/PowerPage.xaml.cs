using IOTOIApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace IOTOIApp.Views
{
    public sealed partial class PowerPage : Page
    {
        private PowerViewModel ViewModel
        {
            get { return DataContext as PowerViewModel; }
        }

        public PowerPage()
        {
            InitializeComponent();
        }
    }
}
