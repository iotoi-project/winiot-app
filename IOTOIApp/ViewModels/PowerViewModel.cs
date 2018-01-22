using System;

using GalaSoft.MvvmLight;
using System.Windows.Input;
using IOTOIApp.Services;
using Windows.UI.Xaml.Controls;
using GalaSoft.MvvmLight.Command;

namespace IOTOIApp.ViewModels
{
    public class PowerViewModel : ViewModelBase
    {
        public ICommand PowerOffCommand { get; private set; }
        public ICommand RestartCommand { get; private set; }


        public PowerViewModel()
        {
            PowerOffCommand = new RelayCommand<ItemClickEventArgs>(PowerOff);
            RestartCommand = new RelayCommand<ItemClickEventArgs>(Restart);
        }

        private void PowerOff(ItemClickEventArgs args)
        {
            CommonService.Shutdown();
        }

        private void Restart(ItemClickEventArgs args)
        {
            CommonService.Restart();
        }
    }
}
