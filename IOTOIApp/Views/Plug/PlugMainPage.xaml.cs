
using System;
using System.Diagnostics;
using IOTOIApp.Utils;
using IOTOIApp.ViewModels.Plug;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace IOTOIApp.Views.Plug
{
    public sealed partial class PlugMainPage : Page
    {
        private PlugMainViewModel ViewModel
        {
            get { return DataContext as PlugMainViewModel; }
        }

        public PlugMainPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Page_Loaded);

            ViewModel.InitDeviceStatusTH();
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            PlugDeivceListView.SelectedIndex = -1;
        }

        private void PlugDeivceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("PlugDeivceListView_SelectionChanged");
            foreach (var item in e.RemovedItems)
            {
                var LightDeivceItem = PlugDeivceListView.ContainerFromItem(item) as ListViewItem;
                var ChildGrid = UIElementUtil.FindChild<Grid>(LightDeivceItem, "ChildGrid");
                if (ChildGrid != null)
                {
                    ChildGrid.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
                }
            }

            foreach (var item in e.AddedItems)
            {
                var LightDeivceItem = PlugDeivceListView.ContainerFromItem(item) as ListViewItem;
                var ChildGrid = UIElementUtil.FindChild<Grid>(LightDeivceItem, "ChildGrid");
                if (ChildGrid != null)
                {
                    ChildGrid.BorderBrush = ConverHexToColor.GetSolidColorBrush("#ffcb00");
                }
            }

        }
    }
}
