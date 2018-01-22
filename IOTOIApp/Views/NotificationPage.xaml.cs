using IOTOIApp.ViewModels;

using Windows.UI.Xaml.Controls;

namespace IOTOIApp.Views
{
    public sealed partial class NotificationPage : Page
    {
        private NotificationViewModel ViewModel
        {
            get { return DataContext as NotificationViewModel; }
        }

        public NotificationPage()
        {
            InitializeComponent();
        }
    }
}
