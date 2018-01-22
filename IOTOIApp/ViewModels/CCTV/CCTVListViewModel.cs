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

        private IOTOI.Model.CCTV _cCTVSelectedItem;
        public IOTOI.Model.CCTV CCTVSelectedItem
        {
            get { return _cCTVSelectedItem; }
            set {
                Set(ref _cCTVSelectedItem, value);
            }
        }

        public CCTVListViewModel()
        {
            //insert test cctv
            using (var db = new Context())
            {
                if(db.CCTV.Count() == 0)
                {
                    db.Add(new IOTOI.Model.CCTV
                    {
                        IpAddress = "192.168.5.171:88",
                        CCTVName = "Foscam1",
                        AccountId = "inslab",
                        AccountPass = "inslab1234",
                        CCTVType = "Foscam"
                    });
                    db.Add(new IOTOI.Model.CCTV
                    {
                        IpAddress = "192.168.5.118",
                        CCTVName = "Commax CCTV1",
                        AccountId = "inslab2",
                        AccountPass = "inslab1234",
                        CCTVType = "Sunell"
                    });
                    db.SaveChanges();
                }
            }

            GetCCTVList();
        }

        public void GetCCTVList()
        {
            using (var db = new Context())
            {
                //List<IOTOI.Model.CCTV> tmpList = db.CCTV.ToList();
                //if (tmpList.Count != 0)
                //{
                //    CCTVListSources = new ObservableCollection<IOTOI.Model.CCTV>();
                //    foreach (IOTOI.Model.CCTV item in tmpList)
                //    {
                //        IOTOI.Model.CCTV ccTv = new IOTOI.Model.CCTV();

                //        ccTv.AccountId = item.AccountId;
                //        ccTv.AccountPass = item.AccountPass;
                //        ccTv.CCTVId = item.CCTVId;
                //        ccTv.CCTVName = item.CCTVName;
                //        ccTv.CCTVType = item.CCTVType;

                //        CCTVListSources.Add(ccTv);
                //    }
            CCTVListSources = new ObservableCollection<IOTOI.Model.CCTV>(db.CCTV.ToList());
            }
        }
    }
}
