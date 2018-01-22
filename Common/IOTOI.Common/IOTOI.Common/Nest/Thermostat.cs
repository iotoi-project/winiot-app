using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace IOTOI.Common.Nest
{
    public class Thermostat : INotifyPropertyChanged
    {
        public string device_id { get { return Reference.device_id; } }
        public string name { get { return Reference.name; } set { Reference.name = value; OnPropertyChanged(); } }
        public string where_name { get { return Reference.where_name; } set { Reference.where_name = value; OnPropertyChanged(); } }

        public bool is_online { get { return Reference.is_online; } set { Reference.is_online = value; OnPropertyChanged(); } }
        public string temperature_scale { get { return Reference.temperature_scale; } set { Reference.temperature_scale = value; OnPropertyChanged(); } }

        //bool temperature_C;
        //public bool temperature_scale_C { get { return temperature_C; } set { temperature_C = value; OnPropertyChanged(); } }

        public double ambient_temperature
        {
            get { return ("C" == temperature_scale) ? Reference.ambient_temperature_c : Reference.ambient_temperature_f; }
            set
            {
                if ("C" == temperature_scale)
                {
                    Reference.ambient_temperature_c = value;
                }
                else
                {
                    Reference.ambient_temperature_f = value;
                }
                OnPropertyChanged();
            }
        }

        public string hvac_mode { get { return Reference.hvac_mode; } set { Reference.hvac_mode = value; OnPropertyChanged(); } }
        public double target_temperature
        {
            get { return ("C" == temperature_scale) ? Reference.target_temperature_c : Reference.target_temperature_f; }
            set
            {
                if ("C" == temperature_scale)
                {
                    Reference.target_temperature_c = value;
                }
                else
                {
                    Reference.target_temperature_f = value;
                }

                ChangeTemperature = true;
                LatestdChangeTime = DateTime.Now;
                OnPropertyChanged();
            }
        }
        public double target_temperature_high
        {
            get { return ("C" == temperature_scale) ? Reference.target_temperature_high_c : Reference.target_temperature_high_f; }
            set
            {
                if ("C" == temperature_scale)
                {
                    Reference.target_temperature_high_c = value;
                }
                else
                {
                    Reference.target_temperature_high_f = value;
                }
                OnPropertyChanged();
            }
        }
        public double target_temperature_low
        {
            get { return ("C" == temperature_scale) ? Reference.target_temperature_low_c : Reference.target_temperature_low_f; }
            set
            {
                if ("C" == temperature_scale)
                {
                    Reference.target_temperature_low_c = value;
                }
                else
                {
                    Reference.target_temperature_low_f = value;
                }
                OnPropertyChanged();
            }
        }

        public bool has_fan { get { return Reference.has_fan; } set { Reference.has_fan = value; OnPropertyChanged(); } }
        public bool fan_timer_active { get { return Reference.fan_timer_active; } set { Reference.fan_timer_active = value; OnPropertyChanged(); } }
        public int fan_timer_duration { get { return Reference.fan_timer_duration; } set { Reference.fan_timer_duration = value; OnPropertyChanged(); } }

        public bool is_locked { get { return Reference.is_locked; } set { Reference.is_locked = value; OnPropertyChanged(); } }
        public double locked_temp_min
        {
            get { return ("C" == temperature_scale) ? Reference.locked_temp_min_c : Reference.locked_temp_min_f; }
            set
            {
                if ("C" == temperature_scale)
                {
                    Reference.locked_temp_min_c = value;
                }
                else
                {
                    Reference.locked_temp_min_f = value;
                }
                OnPropertyChanged();
            }
        }
        public double locked_temp_max
        {
            get { return ("C" == temperature_scale) ? Reference.locked_temp_max_c : Reference.locked_temp_max_f; }
            set
            {
                if ("C" == temperature_scale)
                {
                    Reference.locked_temp_max_c = value;
                }
                else
                {
                    Reference.locked_temp_max_f = value;
                }
                OnPropertyChanged();
            }
        }

        double temperature_min;
        public double set_temperature_min
        {
            get
            {
                if (is_locked)
                {
                    return ("C" == temperature_scale) ? Reference.locked_temp_min_c : Reference.locked_temp_min_f;
                }
                else
                {
                    return ("C" == temperature_scale) ? 9 : 50;
                }
            }
            set
            {
                temperature_min = value;
                OnPropertyChanged();
            }
        }

        double temperature_max;
        public double set_temperature_max
        {
            get
            {
                if (is_locked)
                {
                    return ("C" == temperature_scale) ? Reference.locked_temp_max_c : Reference.locked_temp_max_f;
                }
                else
                {
                    return ("C" == temperature_scale) ? 32 : 90;
                }
            }
            set
            {
                temperature_max = value;
                OnPropertyChanged();
            }
        }




        public bool ChangeTemperature = false;
        public DateTime LatestdChangeTime = new DateTime();

        FanDuration _duration;
        public FanDuration Duration { get { return _duration; } set { _duration = value; OnPropertyChanged(); } }
        public FanDuration RollDuration { get; set; }

        double tickSpacing;
        public double TickSpacing
        {
            get
            {
                return ("C" == temperature_scale) ? 5 : 10;
            }
            set
            {
                tickSpacing = value;
                OnPropertyChanged();
            }
        }

        double _tick;
        public double Tick { get { return _tick; } set { _tick = value; OnPropertyChanged(); } }

        bool _fanRun;
        public bool FanRun
        {
            get { return _fanRun; }
            set
            {
                if (value != _fanRun)
                {
                    _fanRun = value;
                    OnPropertyChanged();

                    FanStop = !value;
                }
            }
        }

        bool _fanStop;
        public bool FanStop
        {
            get { return _fanStop; }
            set
            {
                if (value != _fanStop)
                {
                    _fanStop = value;
                    OnPropertyChanged();

                    FanRun = !value;
                }
            }
        }

        bool _changed;
        public bool Changed { get { return _changed; } set { _changed = value; OnPropertyChanged(); } }


        SolidColorBrush _deviceBrush { get; set; }
        public SolidColorBrush DeviceBrush { get { return _deviceBrush; } set { _deviceBrush = value; OnPropertyChanged(); } }

        SolidColorBrush _deviceBallBrush { get; set; }
        public SolidColorBrush DeviceBallBrush { get { return _deviceBallBrush; } set { _deviceBallBrush = value; OnPropertyChanged(); } }

        SolidColorBrush _devicePrevBallBrush { get; set; }
        public SolidColorBrush DevicePrevBallBrush { get { return _devicePrevBallBrush; } set { _devicePrevBallBrush = value; OnPropertyChanged(); } }

        SolidColorBrush _deviceNextBallBrush { get; set; }
        public SolidColorBrush DeviceNextBallBrush { get { return _devicePrevBallBrush; } set { _devicePrevBallBrush = value; OnPropertyChanged(); } }

        SolidColorBrush _fanBrush { get; set; }
        public SolidColorBrush FanBrush { get { return _fanBrush; } set { _fanBrush = value; OnPropertyChanged(); } }

        SolidColorBrush _fanBallBrush { get; set; }
        public SolidColorBrush FanBallBrush { get { return _fanBallBrush; } set { _fanBallBrush = value; OnPropertyChanged(); } }

        SolidColorBrush _fanPrevBallBrush { get; set; }
        public SolidColorBrush FanPrevBallBrush { get { return _fanPrevBallBrush; } set { _fanPrevBallBrush = value; OnPropertyChanged(); } }

        SolidColorBrush _fanNextBallBrush { get; set; }
        public SolidColorBrush FanNextBallBrush { get { return _fanNextBallBrush; } set { _fanNextBallBrush = value; OnPropertyChanged(); } }




        public Thermostat(ThermostatDevice device)
        {
            Reference = device;

            Changed = false;

            FanBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87));
            FanBallBrush = new SolidColorBrush(Colors.Transparent);

            DeviceBrush = new SolidColorBrush(Color.FromArgb(0xff, 0x87, 0x87, 0x87));

            DeviceBallBrush = new SolidColorBrush(Colors.Transparent);
            DevicePrevBallBrush = new SolidColorBrush(Colors.Transparent);
            DeviceNextBallBrush = new SolidColorBrush(Colors.Transparent);
        }


        public void Update(ThermostatDevice device, FanDuration fduration, bool init = false)
        {
            if (false == init)
            {
                device.Duplicate(Reference);
            }

            Duration = fduration;
            RollDuration = fduration;

            name = Reference.name;
            where_name = Reference.where_name;
            is_online = Reference.is_online;
            temperature_scale = Reference.temperature_scale;
            ambient_temperature = ("C" == temperature_scale) ? Reference.ambient_temperature_c : Reference.ambient_temperature_f;
            hvac_mode = Reference.hvac_mode;
            target_temperature = ("C" == temperature_scale) ? Reference.target_temperature_c : Reference.target_temperature_f;
            target_temperature_high = ("C" == temperature_scale) ? Reference.target_temperature_high_c : Reference.target_temperature_high_f;
            target_temperature_low = ("C" == temperature_scale) ? Reference.target_temperature_low_c : Reference.target_temperature_low_f;
            has_fan = Reference.has_fan;
            fan_timer_active = Reference.fan_timer_active;
            fan_timer_duration = Reference.fan_timer_duration;
            is_locked = Reference.is_locked;
            locked_temp_min = ("C" == temperature_scale) ? Reference.locked_temp_min_c : Reference.locked_temp_min_f;
            locked_temp_max = ("C" == temperature_scale) ? Reference.locked_temp_max_c : Reference.locked_temp_max_f;
            set_temperature_min = 0;
            set_temperature_max = 0;
            TickSpacing = 0;
            Tick = 1; //("C" == temperature_scale) ? 1 : 2;
            FanRun = Reference.fan_timer_active == true;
            FanStop = Reference.fan_timer_active == false;

            Changed = false;
            Changed = true;
        }




        public ThermostatDevice Reference { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }




    public class FanDuration : INotifyPropertyChanged
    {
        public string Duration { get; set; }

        SolidColorBrush _brush { get; set; }
        public SolidColorBrush Brush { get { return _brush; } set { _brush = value; OnPropertyChanged(); } }

        SolidColorBrush _ballBrush { get; set; }
        public SolidColorBrush BallBrush { get { return _ballBrush; } set { _ballBrush = value; OnPropertyChanged(); } }

        SolidColorBrush _prevBallBrush { get; set; }
        public SolidColorBrush PrevBallBrush { get { return _prevBallBrush; } set { _prevBallBrush = value; OnPropertyChanged(); } }

        SolidColorBrush _nextBallBrush { get; set; }
        public SolidColorBrush NextBallBrush { get { return _nextBallBrush; } set { _nextBallBrush = value; OnPropertyChanged(); } }


        public FanDuration()
        {
            Brush = new SolidColorBrush(Color.FromArgb(0xff, 0x7f, 0x7f, 0x7f));

            BallBrush = new SolidColorBrush(Colors.Transparent);
            PrevBallBrush = new SolidColorBrush(Colors.Transparent);
            NextBallBrush = new SolidColorBrush(Colors.Transparent);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
