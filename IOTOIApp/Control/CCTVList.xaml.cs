using IOTOI.Model;
using IOTOIApp.Utils;
using IOTOIApp.ViewModels.CCTV;
using Microsoft.Practices.ServiceLocation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace IOTOIApp.Control
{
    public sealed partial class CCTVList : UserControl
    {
        CCTVListViewModel ViewModel
        {
            get { return DataContext as CCTVListViewModel; }
        }

        public ObservableCollection<CCTV> CCTVListSources
        {
            get { return GetValue(CCTVListSourcesProperty) as ObservableCollection<CCTV>; }
            set { SetValue(CCTVListSourcesProperty, value); }
        }
        public static DependencyProperty CCTVListSourcesProperty = DependencyProperty.Register("CCTVListSources", typeof(ObservableCollection<CCTV>), typeof(CCTVList), new PropertyMetadata(null));

        public CCTV CCTVSelectedItem
        {
            get { return GetValue(CCTVSelectedItemProperty) as CCTV; }
            set { SetValue(CCTVSelectedItemProperty, value); }
        }
        public static DependencyProperty CCTVSelectedItemProperty = DependencyProperty.Register("CCTVSelectedItem", typeof(CCTV), typeof(CCTVList), new PropertyMetadata(null));

        public string PageName
        {
            get { return GetValue(PageNameProperty) as string; }
            set { SetValue(PageNameProperty, value); }
        }
        public static DependencyProperty PageNameProperty = DependencyProperty.Register("PageName", typeof(string), typeof(CCTVList), new PropertyMetadata(null));



        public CCTVList()
        {
            this.InitializeComponent();

            this.Loaded += CCTVList_Loaded;
        }

        private void CCTVList_Loaded(object sender, RoutedEventArgs e)
        {
            var CCTVItem = CCTVListView.ContainerFromItem(CCTVSelectedItem) as ListViewItem;
            var ChildGrid = UIElementUtil.FindChild<Grid>(CCTVItem, "ChildGrid");
            if (ChildGrid != null)
            {
                ChildGrid.BorderBrush = ConverHexToColor.GetSolidColorBrush("#ffcb00");
            }
        }

        private void CCTVListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("CCTVListView_ItemClick!!");

            foreach (Grid gd in UIElementUtil.FindChildArray<Grid>(CCTVListView, "ChildGrid"))
            {
                gd.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
            }

            var item = e.ClickedItem;
            var CCTVItem = CCTVListView.ContainerFromItem(item) as ListViewItem;

            Grid ChildGrid = UIElementUtil.FindChild<Grid>(CCTVItem, "ChildGrid");
            ChildGrid.BorderBrush = ConverHexToColor.GetSolidColorBrush("#ffcb00");

            switch (PageName)
            {
                case "Main":
                    var MainVM = ServiceLocator.Current.GetInstance<CCTVMainViewModel>();
                    MainVM.CCTVSelectedItem = item as IOTOI.Model.CCTV;
                    return;
                case "Setting":
                    var SettingVM = ServiceLocator.Current.GetInstance<CCTVSettingViewModel>();
                    SettingVM.CCTVSelectedItem = item as IOTOI.Model.CCTV;
                    return;
            }
           
        }
    }
}
