using System;

using GalaSoft.MvvmLight;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using Windows.UI.Xaml;
using RavinduL.LocalNotifications;
using RavinduL.LocalNotifications.Presenters;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.ServiceLocation;
using IOTOIApp.Services;
using IOTOI.Model.Db;

namespace IOTOIApp.ViewModels.CCTV
{
    public interface IPage
    {
        void LocalNotice();
    }

    public class CCTVSettingViewModel : ViewModelBase
    {
        public IPage SettingPage { get; set; }

        private NavigationServiceEx NavigationService
        {
            get
            {
                return Microsoft.Practices.ServiceLocation.ServiceLocator.Current.GetInstance<NavigationServiceEx>();
            }
        }

        private IOTOI.Model.CCTV _cCTVSelectedItem = new IOTOI.Model.CCTV();
        public IOTOI.Model.CCTV CCTVSelectedItem
        {
            get { return _cCTVSelectedItem; }
            set { Set(ref _cCTVSelectedItem, value); }
        }

        private string _validationErrorField;
        public string ValidationErrorField
        {
            get { return _validationErrorField; }
            set { Set(ref _validationErrorField, value); }
        }

        private string _validationErrorText;
        public string ValidationErrorText
        {
            get { return _validationErrorText; }
            set { Set(ref _validationErrorText, value); }
        }

        private bool _isSaving = false;
        public bool IsSaving
        {
            get { return _isSaving; }
            set { Set(ref _isSaving, value); }
        }

        public ICommand BackButtonClickedCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand AddCCTVCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        CCTVListViewModel CCTVListVM = ServiceLocator.Current.GetInstance<CCTVListViewModel>();

        public CCTVSettingViewModel()
        {
            BackButtonClickedCommand = new RelayCommand(BackButtonClicked);

            SaveCommand = new RelayCommand(Save);
            AddCCTVCommand = new RelayCommand(AddCCTV);
            DeleteCommand = new RelayCommand(Delete);

            SelectDefaultCCTV();
        }
        private void SelectDefaultCCTV()
        {
            if (CCTVListVM.CCTVListSources.Count > 0)
            {
                CCTVSelectedItem = CCTVListVM.CCTVListSources[0];
            }
        }

        private void BackButtonClicked()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private async void Save()
        {
            Debug.WriteLine("Save!!");
            try
            {
                if (!InputDataValidation()) return;

                IsSaving = true;

                CCTVSelectedItem.CCTVType = await CCTVTypeService.GetCCTVType(CCTVSelectedItem);
                Debug.WriteLine("CCTVType  is !! " + CCTVSelectedItem.CCTVType);

                using (var db = new Context())
                {
                    if (CCTVSelectedItem.CCTVId > 0)
                    {
                        db.Attach(CCTVSelectedItem);
                        db.Update(CCTVSelectedItem);
                    }
                    else
                    {
                        db.Add(CCTVSelectedItem);
                    }

                    db.SaveChanges();
                    Debug.WriteLine("SaveChanges!!");
                }

                IsSaving = false;

                LocalNotice();

                CCTVListVM.GetCCTVList();

                SelectDefaultCCTV();
            }
            catch(Exception e)
            {
                IsSaving = false;
                Debug.WriteLine("Save Exception " + e.Message);
            }
        }

        private bool InputDataValidation()
        {
            ValidationErrorField = "";

            if (string.IsNullOrWhiteSpace(CCTVSelectedItem.IpAddress))
            {
                ValidationErrorField = "IpAddress";
                ValidationErrorText = "* IP Address is Required.";
                return false;
            }
            else if (string.IsNullOrWhiteSpace(CCTVSelectedItem.CCTVName))
            {
                ValidationErrorField = "CCTVName";
                ValidationErrorText = "* Name is Required.";
                return false;
            }
            return true;
        }

        private void AddCCTV()
        {
            CCTVSelectedItem = new IOTOI.Model.CCTV();
        }

        private async void Delete()
        {
            if (CCTVSelectedItem.CCTVId == 0) return;

            ContentDialog deleteCCTVDialog = new ContentDialog
            {
                Title = "Are you sure you want to delete?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };
            ContentDialogResult result = await deleteCCTVDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                using (var db = new Context())
                {
                    db.Remove(CCTVSelectedItem);
                    db.SaveChanges();
                }

                CCTVListVM.GetCCTVList();

                SelectDefaultCCTV();
            }
        }

        public void LocalNotice()
        {
            if (SettingPage == null) return;
            SettingPage.LocalNotice();
        }
    }
}
