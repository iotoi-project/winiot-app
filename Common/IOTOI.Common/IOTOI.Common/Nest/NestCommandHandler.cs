using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTOI.Common.Nest
{
    public static class NestCommandHandler
    {
        public static ThermostatAPI ThermostatAPI { get; private set; }
        static List<Thermostat> DeviceList { get; set; }
        static List<FanDuration> DurationList { get; set; }


        static NestCommandHandler()
        {
            DeviceList = new List<Thermostat>();

            DurationList = new List<FanDuration>()
            {
                new FanDuration() { Duration = "15" },
                new FanDuration() { Duration = "30" },
                new FanDuration() { Duration = "45" },
                new FanDuration() { Duration = "60" },
                new FanDuration() { Duration = "120" },
                new FanDuration() { Duration = "240" },
                new FanDuration() { Duration = "480" },
                new FanDuration() { Duration = "720" }
            };

            ThermostatAPI = new ThermostatAPI();
        }


        public static string CurrentTemperature()
        {
            if (InitDevice() && 0 < DeviceList.Count)
            {
                if (!DeviceList[0].is_online)
                {
                    return "Fail: device off";
                }

                return DeviceList[0].ambient_temperature.ToString();
            }

            return "Fail: no information";
        }


        public static string CurrentFanStatus()
        {
            if (InitDevice() && 0 < DeviceList.Count)
            {
                if (!DeviceList[0].is_online)
                {
                    return "Fail: device off";
                }

                if (!DeviceList[0].has_fan)
                {
                    return "Fail: no fan";
                }

                return DeviceList[0].fan_timer_active ? "FanRunning" : "FanStopped";
            }

            return "Fail: no information";
        }


        static bool InitDevice()
        {
            Task<bool> t = Task.Run(async () => await ThermostatAPI.GetDevices());
            t.Wait();

            if (t.Result)
            {
                int count = DeviceList.Count;
                if (0 == count)
                {
                    foreach (ThermostatDevice d in ThermostatAPI.ThermostatDevices)
                    {
                        FanDuration nfd = DurationList.Where(L => L.Duration == d.fan_timer_duration.ToString()).SingleOrDefault();

                        Thermostat nts = new Thermostat(d);
                        nts.Duration = nfd;
                        nts.RollDuration = nfd;

                        nts.Tick = 1; //("C" == nts.temperature_scale) ? 1 : 2;

                        nts.FanRun = nts.fan_timer_active == true;
                        nts.FanStop = nts.fan_timer_active == false;

                        DeviceList.Add(nts);
                    }
                }
                else
                {
                    if (ThermostatAPI.ThermostatDevices.Count < count)
                    {
                        DeviceList.Clear();
                    }

                    foreach (ThermostatDevice d in ThermostatAPI.ThermostatDevices)
                    {
                        FanDuration nfd = DurationList.Where(L => L.Duration == d.fan_timer_duration.ToString()).SingleOrDefault();

                        Thermostat tm = DeviceList.Where(T => T.device_id == d.device_id).SingleOrDefault();
                        if (null == tm || string.IsNullOrEmpty(tm.device_id))
                        {
                            Thermostat nts = new Thermostat(d);
                            nts.Duration = nfd;
                            nts.RollDuration = nfd;

                            nts.Tick = 1; //("C" == nts.temperature_scale) ? 1 : 2;

                            nts.FanRun = nts.fan_timer_active == true;
                            nts.FanStop = nts.fan_timer_active == false;

                            DeviceList.Add(nts);
                            continue;
                        }

                        if (null != tm || false == string.IsNullOrEmpty(tm.device_id))
                        {
                            tm.Update(d, nfd);
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}
