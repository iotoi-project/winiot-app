using IOTOIApp.Services;
using IOTOIApp.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace IOTOIApp.Views
{
    public sealed partial class ShellPage : UserControl
    {
        private ShellViewModel ViewModel { get { return DataContext as ShellViewModel; } }

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.Initialize(shellFrame);
        }
    }
}
