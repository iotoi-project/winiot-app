using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using IOTOIApp.Helpers;
using IOTOIApp.Services;
using IOTOIApp.Views;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Microsoft.Practices.ServiceLocation;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.System;

namespace IOTOIApp.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private const string PanoramicStateName = "PanoramicState";
        private const string WideStateName = "WideState";
        private const string NarrowStateName = "NarrowState";
        private const double WideStateMinWindowWidth = 640;
        private const double PanoramicStateMinWindowWidth = 1024;

        public NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        private bool _isPaneOpen;
        public bool IsPaneOpen
        {
            get { return _isPaneOpen; }
            set { Set(ref _isPaneOpen, value); }
        }

        private SplitViewDisplayMode _displayMode = SplitViewDisplayMode.CompactInline;
        public SplitViewDisplayMode DisplayMode
        {
            get { return _displayMode; }
            set { Set(ref _displayMode, value); }
        }

        private object _lastSelectedItem;

        private ObservableCollection<ShellNavigationItem> _primaryItems = new ObservableCollection<ShellNavigationItem>();
        public ObservableCollection<ShellNavigationItem> PrimaryItems
        {
            get { return _primaryItems; }
            set { Set(ref _primaryItems, value); }
        }

        private ObservableCollection<ShellNavigationItem> _secondaryItems = new ObservableCollection<ShellNavigationItem>();
        public ObservableCollection<ShellNavigationItem> SecondaryItems
        {
            get { return _secondaryItems; }
            set { Set(ref _secondaryItems, value); }
        }

        private Visibility _hamburgerVisibility;
        public Visibility HamburgerVisibility
        {
            get { return _hamburgerVisibility; }
            set { Set(ref _hamburgerVisibility, value); }
        }

        private ICommand _openPaneCommand;
        public ICommand OpenPaneCommand
        {
            get
            {
                if (_openPaneCommand == null)
                {
                    _openPaneCommand = new RelayCommand(() => IsPaneOpen = !_isPaneOpen);
                }

                return _openPaneCommand;
            }
        }

        private ICommand _itemSelected;
        public ICommand ItemSelectedCommand
        {
            get
            {
                if (_itemSelected == null)
                {
                    _itemSelected = new RelayCommand<ItemClickEventArgs>(ItemSelected);
                }

                return _itemSelected;
            }
        }

        private ICommand _stateChangedCommand;
        public ICommand StateChangedCommand
        {
            get
            {
                if (_stateChangedCommand == null)
                {
                    _stateChangedCommand = new RelayCommand<Windows.UI.Xaml.VisualStateChangedEventArgs>(args => GoToState(args.NewState.Name));
                }

                return _stateChangedCommand;
            }
        }

        private Visibility _rightPanelVisibility = Visibility.Visible;
        public Visibility RightPanelVisibility
        {
            get { return _rightPanelVisibility; }
            set { Set(ref _rightPanelVisibility, value); }
        }

        private Visibility _footerPageVisibility = Visibility.Visible;
        public Visibility FooterPageVisibility
        {
            get { return _footerPageVisibility; }
            set { Set(ref _footerPageVisibility, value); }
        }

        private void GoToState(string stateName)
        {
            switch (stateName)
            {
                case PanoramicStateName:
                    HamburgerVisibility = Visibility.Collapsed;
                    DisplayMode = SplitViewDisplayMode.CompactOverlay;
                    break;
                case WideStateName:
                    HamburgerVisibility = Visibility.Collapsed;
                    DisplayMode = SplitViewDisplayMode.CompactOverlay;
                    IsPaneOpen = false;
                    break;
                case NarrowStateName:
                    HamburgerVisibility = Visibility.Visible;
                    DisplayMode = SplitViewDisplayMode.Overlay;
                    IsPaneOpen = false;
                    break;
                default:
                    break;
            }
        }

        public void Initialize(Frame frame)
        {
            NavigationService.Frame = frame;
            NavigationService.Frame.Navigated += NavigationService_Navigated;
            PopulateNavItems();

            InitializeState(Window.Current.Bounds.Width);
        }

        private void InitializeState(double windowWith)
        {
            if (windowWith < WideStateMinWindowWidth)
            {
                GoToState(NarrowStateName);
            }
            else if (windowWith < PanoramicStateMinWindowWidth)
            {
                GoToState(WideStateName);
            }
            else
            {
                GoToState(PanoramicStateName);
            }
        }

        //public enum Symbol;

        private void PopulateNavItems()
        {
            _primaryItems.Clear();
            _secondaryItems.Clear();

            // TODO WTS: Change the symbols for each item as appropriate for your app
            // More on Segoe UI Symbol icons: https://docs.microsoft.com/windows/uwp/style/segoe-ui-symbol-font
            // Or to use an IconElement instead of a Symbol see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/projectTypes/navigationpane.md
            // Edit String/en-US/Resources.resw: Add a menu item title for each page
            //_primaryItems.Add(new ShellNavigationItem("Shell_Main".GetLocalized(), Symbol.Document, typeof(MainViewModel).FullName));
            //_secondaryItems.Add(new ShellNavigationItem("Shell_Settings".GetLocalized(), Symbol.Setting, typeof(SettingsViewModel).FullName));
            //_primaryItems.Add(new ShellNavigationItem("Shell_Notification".GetLocalized(), Symbol.Document, typeof(NotificationViewModel).FullName));
            //_primaryItems.Add(new ShellNavigationItem("Shell_Cortana".GetLocalized(), Symbol.Document, typeof(CortanaViewModel).FullName));
            //_primaryItems.Add(new ShellNavigationItem("Shell_AppList".GetLocalized(), Symbol.AllApps, typeof(AppListViewModel).FullName));

            //_primaryItems.Add(new ShellNavigationItem("Shell_Main".GetLocalized(), "\uE80F", typeof(MainViewModel).FullName));

           // _primaryItems.Add(new ShellNavigationItem("Shell_AppList".GetLocalized(), "\uE71D", typeof(AppListViewModel).FullName));
            //_primaryItems.Add(new ShellNavigationItem("Shell_Cortana".GetLocalized(), "\uE1D2", typeof(CortanaViewModel).FullName));
            //_primaryItems.Add(new ShellNavigationItem("Shell_Dashboard".GetLocalized(), "\uE81E", typeof(DashboardViewModel).FullName));

            //_secondaryItems.Add(new ShellNavigationItem("Shell_Notification".GetLocalized(), "\uEDAC", typeof(NotificationViewModel).FullName));
            //_secondaryItems.Add(new ShellNavigationItem("Shell_Settings".GetLocalized(), "\uE115", typeof(SettingsViewModel).FullName));
           // _secondaryItems.Add(new ShellNavigationItem("Shell_Power".GetLocalized(), "\uE7E8", typeof(PowerViewModel).FullName));

            _primaryItems.Add(new ShellNavigationItem("Shell_AppList".GetLocalized(), new Uri("ms-appx:///Assets/svg/apps-icon.svg"), typeof(AppListViewModel).FullName));
            _primaryItems.Add(new ShellNavigationItem("Shell_Cortana".GetLocalized(), new Uri("ms-appx:///Assets/svg/cortana-icon.svg"), typeof(CortanaViewModel).FullName));
        }

        private void ItemSelected(ItemClickEventArgs args)
        {
            if (DisplayMode == SplitViewDisplayMode.CompactOverlay || DisplayMode == SplitViewDisplayMode.Overlay)
            {
                IsPaneOpen = false;
            }

            var navigationItem = args.ClickedItem as ShellNavigationItem;
            if(navigationItem.Label == "Cortana")
            {
                LaunchCortana();
            }
            else
            {
                Navigate(args.ClickedItem);
            }
        }

        ShellNavigationItem CurrentFrameViewModel;
        
        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            if (e != null)
            {
                var vm = NavigationService.GetNameOfRegisteredPage(e.SourcePageType);
                var navigationItem = PrimaryItems?.FirstOrDefault(i => i.ViewModelName == vm);
                if (navigationItem == null)
                {
                    navigationItem = SecondaryItems?.FirstOrDefault(i => i.ViewModelName == vm);
                }

                if (navigationItem != null)
                {
                    ChangeSelected(_lastSelectedItem, navigationItem);
                    _lastSelectedItem = navigationItem;
                }

                if(navigationItem == null && CurrentFrameViewModel != null)
                {
                    (CurrentFrameViewModel as ShellNavigationItem).IsSelected = false;
                }
                
                CurrentFrameViewModel = navigationItem;
            }
        }

        private void ChangeSelected(object oldValue, object newValue)
        {
            if (oldValue != null)
            {
                (oldValue as ShellNavigationItem).IsSelected = false;
            }
            if (newValue != null)
            {
                (newValue as ShellNavigationItem).IsSelected = true;
            }
        }

        private void Navigate(object item)
        {
            var navigationItem = item as ShellNavigationItem;
            if (navigationItem != null)
            {
                NaviToSettingPage(navigationItem.Label == "Settings");

                //현재 활성화된 메뉴 다시 누를시 Main으로 진입, 메뉴버튼은 toggle 형식으로 봄
                if(CurrentFrameViewModel == navigationItem) {
                    (CurrentFrameViewModel as ShellNavigationItem).IsSelected = false;
                    NavigationService.Navigate("IOTOIApp.ViewModels.MainViewModel");
                } else
                {
                    NavigationService.Navigate(navigationItem.ViewModelName);
                }
            }
        }

        public void NaviToSettingPage(bool val)
        {
            if (val)
            {
                RightPanelVisibility = Visibility.Collapsed;
                HamburgerVisibility = Visibility.Collapsed;
                FooterPageVisibility = Visibility.Collapsed;
                DisplayMode = SplitViewDisplayMode.Overlay;
                IsPaneOpen = false;
            }
            else
            {
                RightPanelVisibility = Visibility.Visible;
                HamburgerVisibility = Visibility.Visible;
                FooterPageVisibility = Visibility.Visible;
                InitializeState(Window.Current.Bounds.Width);
            }
        }

        public async void LaunchCortana()
        {
            await Launcher.LaunchUriAsync(new Uri("ms-cortana://"));
        }
    }
}
