using IOTOI.Model.Db;
using IOTOI.Model;
using GalaSoft.MvvmLight;
using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTOIApp.ViewModels.CCTV
{
    public class CCTVListViewModel : ViewModelBase
    {
        private ObservableCollection<IOTOI.Model.CCTV> _cCTVListSources = new ObservableCollection<IOTOI.Model.CCTV>();
        public ObservableCollection<IOTOI.Model.CCTV> CCTVListSources
        {
            get { return _cCTVListSources; }
            set { Set(ref _cCTVListSources, value); }
        }

        public CCTVListViewModel()
        {
        }

        public void GetCCTVList()
        {
            using (var db = new Context())
            {
                CCTVListSources = new ObservableCollection<IOTOI.Model.CCTV>(db.CCTV.ToList());
            }
        }
    }
}
