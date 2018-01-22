using System;

using IOTOIApp.ViewModels.Light;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml.Navigation;
using IOTOIApp.Utils;

namespace IOTOIApp.Views.Light
{
    public sealed partial class LightMainPage : Page
    {
        private LightMainViewModel ViewModel
        {
            get { return DataContext as LightMainViewModel; }
        }

        public LightMainPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Page_Loaded);

            ViewModel.InitDeviceStatusTH();
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LightDeivceListView.SelectedIndex = -1;
        }

        private void LightDeivceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("LightDeivceListView_SelectionChanged");
            foreach (var item in e.RemovedItems)
            {
                var LightDeivceItem = LightDeivceListView.ContainerFromItem(item) as ListViewItem;
                var ChildGrid = UIElementUtil.FindChild<Grid>(LightDeivceItem, "ChildGrid");
                if (ChildGrid != null)
                {
                    ChildGrid.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
                }
            }

            foreach (var item in e.AddedItems)
            {
                var LightDeivceItem = LightDeivceListView.ContainerFromItem(item) as ListViewItem;
                var ChildGrid = UIElementUtil.FindChild<Grid>(LightDeivceItem, "ChildGrid");
                if (ChildGrid != null)
                {
                    ChildGrid.BorderBrush = new SolidColorBrush(Windows.UI.Colors.OrangeRed);
                }
            }

        }

    }
}
