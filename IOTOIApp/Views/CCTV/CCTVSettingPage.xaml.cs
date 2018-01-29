using System;

using Windows.UI.Xaml.Controls;
using RavinduL.LocalNotifications;
using RavinduL.LocalNotifications.Presenters;
using Windows.UI.Xaml.Media;
using Windows.UI.Popups;
using System.Diagnostics;
using IOTOIApp.ViewModels.CCTV;
using IOTOIApp.Utils;

namespace IOTOIApp.Views.CCTV
{
    public sealed partial class CCTVSettingPage : Page, IPage
    {
        private CCTVSettingViewModel ViewModel
        {
            get { return DataContext as CCTVSettingViewModel; }
        }

        private LocalNotificationManager NotiManager;

        public CCTVSettingPage()
        {
            InitializeComponent();

            ViewModel.SelectDefaultCCTV();

            ViewModel.SettingPage = this as IPage;

            NotiManager = new LocalNotificationManager(NotificationGrid);
            
        }

        public void LocalNotice()
        {
            string NoticeText = "Saved.";
            NotiManager.Show(new SimpleNotificationPresenter
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

    }
}
