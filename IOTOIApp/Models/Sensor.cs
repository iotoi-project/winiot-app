using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace IOTOIApp.Models
{
    public class Sensor : INotifyPropertyChanged
    {
        /// <summary>
        /// 0 : Motion Sensor, 1 : Magnetic
        /// </summary>
        public int SensorType { get; set; }
        public int SensorId { get; set; }
        public short SensorNo { get; set; }
        public string SensorName { get; set; }
        public string SensorViewTitle { get; set; }


        /// <summary>
        /// 0 : Normal, 1 : Warning, 2 : Offline
        /// </summary>
        int _sensorStatus;
        public int SensorStatus
        {
            get { return _sensorStatus; }
            set
            {
                _sensorStatus = value;
                OnPropertyChanged();

                TitleColor = new SolidColorBrush(Colors.White);
                ViewColor = new SolidColorBrush(Colors.White);
                WarningIcon = Visibility.Collapsed;

                OfflineText = value == 2 ? "It does not work." : "";

                if (0 == SensorType)
                {
                    switch (value)
                    {
                        case 0:
                            MotionNormal = Visibility.Visible;
                            MotionWarning = Visibility.Collapsed;
                            MotionOffline = Visibility.Collapsed;

                            DoorNormal = Visibility.Collapsed;
                            DoorWarning = Visibility.Collapsed;
                            DoorOffline = Visibility.Collapsed;
                            break;

                        case 1:
                            WarningIcon = Visibility.Visible;
                            ViewColor = new SolidColorBrush(Color.FromArgb(0xff, 0xe7, 0x51, 0x3e));
                            MotionNormal = Visibility.Collapsed;
                            MotionWarning = Visibility.Visible;
                            MotionOffline = Visibility.Collapsed;

                            DoorNormal = Visibility.Collapsed;
                            DoorWarning = Visibility.Collapsed;
                            DoorOffline = Visibility.Collapsed;
                            break;

                        case 2:
                            TitleColor = new SolidColorBrush(Color.FromArgb(0x4c, 0xff, 0xff, 0xff));
                            ViewColor = new SolidColorBrush(Color.FromArgb(0x2c, 0xff, 0xff, 0xff));
                            MotionNormal = Visibility.Collapsed;
                            MotionWarning = Visibility.Collapsed;
                            MotionOffline = Visibility.Visible;

                            DoorNormal = Visibility.Collapsed;
                            DoorWarning = Visibility.Collapsed;
                            DoorOffline = Visibility.Collapsed;
                            break;
                    }
                }
                else
                {
                    switch (value)
                    {
                        case 0:
                            MotionNormal = Visibility.Collapsed;
                            MotionWarning = Visibility.Collapsed;
                            MotionOffline = Visibility.Collapsed;

                            DoorNormal = Visibility.Visible;
                            DoorWarning = Visibility.Collapsed;
                            DoorOffline = Visibility.Collapsed;
                            break;
                        case 1:
                            WarningIcon = Visibility.Visible;
                            ViewColor = new SolidColorBrush(Color.FromArgb(0xff, 0xe7, 0x51, 0x3e));
                            MotionNormal = Visibility.Collapsed;
                            MotionWarning = Visibility.Collapsed;
                            MotionOffline = Visibility.Collapsed;

                            DoorNormal = Visibility.Collapsed;
                            DoorWarning = Visibility.Visible;
                            DoorOffline = Visibility.Collapsed;
                            break;

                        case 2:
                            TitleColor = new SolidColorBrush(Color.FromArgb(0x4c, 0xff, 0xff, 0xff));
                            ViewColor = new SolidColorBrush(Color.FromArgb(0x2c, 0xff, 0xff, 0xff));
                            MotionNormal = Visibility.Collapsed;
                            MotionWarning = Visibility.Collapsed;
                            MotionOffline = Visibility.Collapsed;

                            DoorNormal = Visibility.Collapsed;
                            DoorWarning = Visibility.Collapsed;
                            DoorOffline = Visibility.Visible;
                            break;
                    }
                }
            }
        }

        SolidColorBrush _viewColor;
        public SolidColorBrush ViewColor
        {
            get { return _viewColor; }
            set { _viewColor = value; OnPropertyChanged(); }
        }

        SolidColorBrush _titleColor;
        public SolidColorBrush TitleColor
        {
            get { return _titleColor; }
            set { _titleColor = value; OnPropertyChanged(); }
        }

        Visibility _warningIcon;
        public Visibility WarningIcon
        {
            get { return _warningIcon; }
            set { _warningIcon = value; OnPropertyChanged(); }
        }


        Visibility _motionNormal;
        public Visibility MotionNormal
        {
            get { return _motionNormal; }
            set { _motionNormal = value; OnPropertyChanged(); }
        }

        Visibility _motionWarning;
        public Visibility MotionWarning
        {
            get { return _motionWarning; }
            set { _motionWarning = value; OnPropertyChanged(); }
        }

        Visibility _motionOffline;
        public Visibility MotionOffline
        {
            get { return _motionOffline; }
            set { _motionOffline = value; OnPropertyChanged(); }
        }

        Visibility _doorNormal;
        public Visibility DoorNormal
        {
            get { return _doorNormal; }
            set { _doorNormal = value; OnPropertyChanged(); }
        }

        Visibility _doorWarning;
        public Visibility DoorWarning
        {
            get { return _doorWarning; }
            set { _doorWarning = value; OnPropertyChanged(); }
        }

        Visibility _doorOffline;
        public Visibility DoorOffline
        {
            get { return _doorOffline; }
            set { _doorOffline = value; OnPropertyChanged(); }
        }

        //public string SensorIcon
        //{
        //    get { return (0 == SensorType) ? Application.Current.Resources["IconMotion"].ToString() :
        //                                     Application.Current.Resources["IconMagnetic"].ToString(); }
        //}

        string _offlineText;
        public string OfflineText
        {
            get { return _offlineText; }
            set { _offlineText = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Sensor()
        {
            //SensorStatus = 0;
            //SensorViewTitle = "Main Room";
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
