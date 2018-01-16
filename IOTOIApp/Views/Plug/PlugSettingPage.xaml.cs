
using System;
using System.Diagnostics;
using IOTOI.Model.ZigBee;
using IOTOIApp.Utils;
using IOTOIApp.ViewModels.Plug;
using RavinduL.LocalNotifications;
using RavinduL.LocalNotifications.Presenters;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace IOTOIApp.Views.Plug
{
    public sealed partial class PlugSettingPage : Page
    {
        private PlugSettingViewModel ViewModel
        {
            get { return DataContext as PlugSettingViewModel; }
        }

        public PlugSettingPage()
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

        private void SettingPlugListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("SettingPlugListView_ItemClick!!");

            foreach (TextBlock tb in UIElementUtil.FindChildArray<TextBlock>(SettingDeviceListView, "CircleIcon"))
            {
                tb.Foreground = new SolidColorBrush(Windows.UI.Colors.DimGray);
            }

            ListView SettingPlugListView = sender as ListView;
            var item = e.ClickedItem;
            var PlugItem = SettingPlugListView.ContainerFromItem(item) as ListViewItem;

            foreach (TextBlock tb in UIElementUtil.FindChildArray<TextBlock>(PlugItem, "CircleIcon"))
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
            ListView SettingPlugListView = UIElementUtil.FindParent<ListView>((TextBox)sender);

            var PlugItem = (ListViewItem)SettingPlugListView.ContainerFromItem(endpoint);
            foreach (TextBlock tb in UIElementUtil.FindChildArray<TextBlock>(PlugItem, "CircleIcon"))
            {
                tb.Foreground = ConverHexToColor.GetSolidColorBrush("#ffcb00");
            }
        }
    }
}
