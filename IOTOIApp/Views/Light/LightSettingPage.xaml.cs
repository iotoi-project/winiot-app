using System;

using IOTOIApp.ViewModels.Light;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using RavinduL.LocalNotifications;
using RavinduL.LocalNotifications.Presenters;
using Windows.UI.Popups;
using IOTOI.Model.ZigBee;
using System.Diagnostics;
using IOTOIApp.Utils;

namespace IOTOIApp.Views.Light
{
    public sealed partial class LightSettingPage : Page
    {
        private LightSettingViewModel ViewModel
        {
            get { return DataContext as LightSettingViewModel; }
        }

        public LightSettingPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(Page_Loaded);
        }

        void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SettingDeviceListView.SelectedIndex = -1;

            manager = new LocalNotificationManager(NotificationGrid);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            LocalNotice();
        }

        private LocalNotificationManager manager;
        private void LocalNotice()
        {
            string NoticeText = "Saved.";
            manager.Show(new SimpleNotificationPresenter
            (
                TimeSpan.FromSeconds(2),
                text: NoticeText,
                action: async () => await new MessageDialog(NoticeText).ShowAsync(),
                glyph: "\uE001"
            )
            {
                Background = ConverHexToColor.GetSolidColorBrush("#ffcb00"),
                Foreground = new SolidColorBrush(Windows.UI.Colors.White),
            },
            (LocalNotificationCollisionBehaviour)1);
        }

        private void SettingLightListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("SettingLightListView_ItemClick!!");

            foreach (TextBlock tb in UIElementUtil.FindChildArray<TextBlock>(SettingDeviceListView, "CircleIcon"))
            {
                tb.Foreground = new SolidColorBrush(Windows.UI.Colors.DimGray);
            }

            ListView SettingLightListView = sender as ListView;
            var item = e.ClickedItem;
            var LightItem = SettingLightListView.ContainerFromItem(item) as ListViewItem;

            foreach (TextBlock tb in UIElementUtil.FindChildArray<TextBlock>(LightItem, "CircleIcon"))
            {
                tb.Foreground = ConverHexToColor.GetSolidColorBrush("#ffcb00");
            }
        }


        private void CustomName_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("CustomName_GotFocus!!");
            foreach (TextBlock tb in UIElementUtil.FindChildArray<TextBlock>(SettingDeviceListView, "CircleIcon"))
            {
                tb.Foreground = new SolidColorBrush(Windows.UI.Colors.DimGray);
            }

            TextBox textBox = sender as TextBox;
            ZigBeeEndPoint endpoint = textBox.DataContext as ZigBeeEndPoint;
            ListView SettingLightListView = UIElementUtil.FindParent<ListView>((TextBox)sender);

            var LightItem = (ListViewItem)SettingLightListView.ContainerFromItem(endpoint);
            foreach (TextBlock tb in UIElementUtil.FindChildArray<TextBlock>(LightItem, "CircleIcon"))
            {
                tb.Foreground = ConverHexToColor.GetSolidColorBrush("#ffcb00");
            }
        }
        
    }
}
