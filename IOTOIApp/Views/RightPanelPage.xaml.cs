using IOTOIApp.ViewModels;

using Windows.UI.Xaml.Controls;

// 빈 페이지 항목 템플릿에 대한 설명은 https://go.microsoft.com/fwlink/?LinkId=234238에 나와 있습니다.

namespace IOTOIApp.Views
{
    /// <summary>
    /// 자체적으로 사용하거나 프레임 내에서 탐색할 수 있는 빈 페이지입니다.
    /// </summary>

    public sealed partial class RightPanelPage : UserControl
    {
        private RightPanelViewModel ViewModel
        {
            get { return DataContext as RightPanelViewModel; }
        }

        public RightPanelPage()
        {
            this.InitializeComponent();
        }
    }
}
