using System;

using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.Net.Http;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using System.Net;
using System.IO;
using System.Diagnostics;
using Windows.System.Threading;
using Windows.UI.Core;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;
using Windows.UI.Popups;
using System.Threading;
using Windows.UI.Xaml;
using IOTOIApp.Services;
using Microsoft.Practices.ServiceLocation;
using IOTOIApp.ViewModels;

namespace IOTOIApp.ViewModels.CCTV
{
    public class CCTVMainViewModel : ViewModelBase
    {
        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        private IOTOI.Model.CCTV _cCTVSelectedItem;
        public IOTOI.Model.CCTV CCTVSelectedItem
        {
            get { return _cCTVSelectedItem; }
            set { Set(ref _cCTVSelectedItem, value);
                StartImageStream();
            }
        }

        private ImageSource _streamImage;
        public ImageSource StreamImage
        {
            get { return _streamImage; }
            set { Set(ref _streamImage, value); }
        }

        private bool _inProgress = false;
        public bool InProgress
        {
            get { return _inProgress; }
            set { Set(ref _inProgress, value); }
        }

        private Visibility _startBtnVisibility = Visibility.Collapsed;
        public Visibility StartBtnVisibility
        {
            get { return _startBtnVisibility; }
            set { Set(ref _startBtnVisibility, value); }
        }

        public ICommand BackButtonClickedCommand { get; private set; }
        public ICommand GoSettingsPageCommand { get; private set; }
        public ICommand StartImageStreamCommand { get; private set; }
        public ICommand StopImageStreamCommand { get; private set; }

        CCTVListViewModel CCTVListVM = ServiceLocator.Current.GetInstance<CCTVListViewModel>();
        private int SelectedIndex = 0;

        public CCTVMainViewModel()
        {
            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);
            GoSettingsPageCommand = new RelayCommand(GoSettingsPage);
            StartImageStreamCommand = new RelayCommand(StartImageStream);
            StopImageStreamCommand = new RelayCommand(StopImageStream);

            SelectDefaultCCTV();
        }

        public void SelectDefaultCCTV()
        {
            InProgress = false;
            StreamImage = null;

            CCTVListVM.GetCCTVList();

            if(CCTVListVM.CCTVListSources.Count > 0)
            {
                if ((CCTVListVM.CCTVListSources.Count - 1) < SelectedIndex)
                {
                    SelectedIndex = (CCTVListVM.CCTVListSources.Count - 1);
                }
                CCTVSelectedItem = CCTVListVM.CCTVListSources[SelectedIndex];
                StartImageStream();
            }
        }

        private static ThreadPoolTimer PeriodicTimer;
        private static bool RunImageStreamTimer;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        
        public void StartImageStream()
        {
            if (CCTVSelectedItem == null) return;
            if (PeriodicTimer != null) PeriodicTimer.Cancel();

            SelectedIndex = CCTVListVM.CCTVListSources.IndexOf(CCTVSelectedItem);

            StartBtnVisibility = Visibility.Collapsed;
            InProgress = true;
            StreamImage = null;
            _cts.Cancel();
            _cts = new CancellationTokenSource();

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://" + CCTVSelectedItem.IpAddress);
            httpClient.Timeout = TimeSpan.FromMilliseconds(5000);
            string RequestUri = CCTVSelectedItem.CgiUrl;

            Debug.WriteLine("RequestUri :: " + RequestUri);

            HttpResponseMessage response = new HttpResponseMessage();
            BitmapImage bitmap = new BitmapImage();

            RunImageStreamTimer = true;

            TimeSpan period = TimeSpan.FromMilliseconds(200);
            PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    Debug.WriteLine("RunImageStreamTimer :: " + RunImageStreamTimer);
                    if (RunImageStreamTimer)
                    {
                        try
                        {
                            RunImageStreamTimer = false;

                            response = await httpClient.GetAsync(RequestUri, _cts.Token);

                            if (response != null && response.StatusCode == HttpStatusCode.OK)
                            {
                                using (var stream = await response.Content.ReadAsStreamAsync())
                                {
                                    using (var memStream = new MemoryStream())
                                    {
                                        await stream.CopyToAsync(memStream);
                                        memStream.Position = 0;
                                        bitmap.SetSource(memStream.AsRandomAccessStream());
                                    }
                                }
                                StreamImage = bitmap;
                                if(InProgress) InProgress = false;
                            }
                            RunImageStreamTimer = true;
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message, "GetImageStream Exception : ");
                            RunImageStreamTimer = true;
                        }
                    }
                });

            }, period);
        }

        private void StopImageStream()
        {
            if (PeriodicTimer != null) PeriodicTimer.Cancel();
            StartBtnVisibility = Visibility.Visible;
        }

        private void GoSettingsPage()
        {
            NavigationService.Navigate("IOTOIApp.ViewModels.CCTV.CCTVSettingViewModel");
            if (PeriodicTimer != null) PeriodicTimer.Cancel();
        }

        private void BackButtonClicked()
        {
            if (NavigationService.CanGoBack)
            {
                var ShellVM = ServiceLocator.Current.GetInstance<ShellViewModel>();
                ShellVM.NaviToSettingPage(false);
                NavigationService.GoBack();

                if (PeriodicTimer != null) PeriodicTimer.Cancel();
            }
        }
    }
}
