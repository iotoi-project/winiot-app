using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace IOTOIApp.Models
{
    public class CmxApp
    {
        public string AppName { get; set; }
        public string AppIcon { get; set; }
        public Uri AppIconSvgUri { get; set; }
        public ICommand AppLinkCommand { get; set; }
        public string AppLinkParam { get; set; }
        public Visibility TextIconVisibility
        {
            get { return string.IsNullOrEmpty(AppIcon) ? Visibility.Collapsed : Visibility.Visible; }
        }
    }

   public class CmxAppPivot
    {
        public string Title { get; set; }
        public ObservableCollection<CmxApp> PivotAppList { get; set; }
    }
}
