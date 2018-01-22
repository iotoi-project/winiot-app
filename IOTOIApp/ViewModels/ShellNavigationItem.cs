using GalaSoft.MvvmLight;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace IOTOIApp.ViewModels
{
    public class ShellNavigationItem : ViewModelBase
    {
        private bool _isSelected;

        private Visibility _selectedVis = Visibility.Collapsed;
        public Visibility SelectedVis
        {
            get { return _selectedVis; }
            set { Set(ref _selectedVis, value); }
        }

        private SolidColorBrush _selectedForeground = null;
        public SolidColorBrush SelectedForeground
        {
            get
            {
                return _selectedForeground ?? (_selectedForeground = GetStandardTextColorBrush());
            }
            set { Set(ref _selectedForeground, value); }
        }

        public string Label { get; set; }
        public Symbol Symbol { get; set; }
        public char SymbolAsChar { get { return (char)Symbol; } }
        public string Symbol2 { get; set; }

        public string ViewModelName { get; set; }

        private IconElement _iconElement = null;
        public IconElement Icon
        {
            get
            {
                var foregroundBinding = new Binding
                {
                    Source = this,
                    Path = new PropertyPath("SelectedForeground"),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                if (_iconElement != null)
                {
                    BindingOperations.SetBinding(_iconElement, IconElement.ForegroundProperty, foregroundBinding);

                    return _iconElement;
                }

                var fontIcon = new FontIcon { FontSize = 16, Glyph = Symbol2 };

                BindingOperations.SetBinding(fontIcon, FontIcon.ForegroundProperty, foregroundBinding);

                return fontIcon;
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                Set(ref _isSelected, value);
                SelectedVis = value ? Visibility.Visible : Visibility.Collapsed;
                SelectedForeground = value
                //    ? Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush
                        ? new SolidColorBrush(Windows.UI.Colors.Cyan)
                    : GetStandardTextColorBrush();
            }
        }
        public Uri SvgUri { get; set; }

        private SolidColorBrush GetStandardTextColorBrush()
        {
            var brush = Application.Current.Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;

            if (!Services.ThemeSelectorService.IsLightThemeEnabled)
            {
                brush = Application.Current.Resources["SystemControlForegroundAltHighBrush"] as SolidColorBrush;
            }

            return brush;
        }

        public ShellNavigationItem(string label, Symbol symbol, string viewModelName)
        {
            this.Label = label;
            this.Symbol = symbol;
            this.ViewModelName = viewModelName;

            Services.ThemeSelectorService.OnThemeChanged += (s, e) => { if (!IsSelected) SelectedForeground = GetStandardTextColorBrush(); };
        }

        public ShellNavigationItem(string label, IconElement icon, string viewModelName)
        {
            this.Label = label;
            this._iconElement = icon;
            this.ViewModelName = viewModelName;

            Services.ThemeSelectorService.OnThemeChanged += (s, e) => { if (!IsSelected) SelectedForeground = GetStandardTextColorBrush(); };
        }

        public ShellNavigationItem(string label, string symbol, string viewModelName)
        {
            this.Label = label;
            this.Symbol2 = symbol;
            this.ViewModelName = viewModelName;

            Services.ThemeSelectorService.OnThemeChanged += (s, e) => { if (!IsSelected) SelectedForeground = GetStandardTextColorBrush(); };
        }

        public ShellNavigationItem(string label, Uri svgUri, string viewModelName)
        {
            this.Label = label;
            this.SvgUri = svgUri;
            this.ViewModelName = viewModelName;

            Services.ThemeSelectorService.OnThemeChanged += (s, e) => { if (!IsSelected) SelectedForeground = GetStandardTextColorBrush(); };
        }

    }
}
