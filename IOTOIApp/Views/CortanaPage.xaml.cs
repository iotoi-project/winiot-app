using IOTOIApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace IOTOIApp.Views
{
    public sealed partial class CortanaPage : Page
    {
        private CortanaViewModel ViewModel
        {
            get { return DataContext as CortanaViewModel; }
        }

        public CortanaPage()
        {
            InitializeComponent();
        }
    }
}
