using IOTOI.Model;
using IOTOIApp.Utils;
using IOTOIApp.ViewModels.CCTV;
using Microsoft.Practices.ServiceLocation;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
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
            set { SetValue(CCTVSelectedItemProperty, value); HighLightSelectedItem(value); }
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

        private async void HighLightSelectedItem(object Item)
        {
            if (Item == null) return;

            await Task.Delay(100);

            foreach (Grid gd in UIElementUtil.FindChildArray<Grid>(CCTVListView, "ChildGrid"))
            {
                gd.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
            }

            var CCTVItem = CCTVListView.ContainerFromItem(Item) as ListViewItem;
            var ChildGrid = UIElementUtil.FindChild<Grid>(CCTVItem, "ChildGrid");
            if (ChildGrid != null)
            {
                ChildGrid.BorderBrush = ConverHexToColor.GetSolidColorBrush("#ffcb00");
            }
        }

        private void CCTVListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("CCTVListView_ItemClick!!");

            var Item = e.ClickedItem;

            switch (PageName)
            {
                case "Main":
                    var MainVM = ServiceLocator.Current.GetInstance<CCTVMainViewModel>();
                    MainVM.CCTVSelectedItem = Item as IOTOI.Model.CCTV;
                    return;
                case "Setting":
                    var SettingVM = ServiceLocator.Current.GetInstance<CCTVSettingViewModel>();
                    SettingVM.CCTVSelectedItem = Item as IOTOI.Model.CCTV;
                    return;
            }
           
        }
    }
}
