using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTOI.Common.Nest
{
    public class DeviceParse
    {
        public List<ThermostatDevice> ThermostatDevices { get; set; }
        public List<Structure> Structures { get; set; }


        /// <summary>
        /// is_locked, lock min - max, can_cool, can_heat  -- readonly
        /// is_locked true --> hvac_mode can change
        /// hvac_mode --> heat, cool, heat-cool, eco, off
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public bool Parse(string json)
        {
            try
            {
                dynamic dynObj = JsonConvert.DeserializeObject(json);

                if (null == ThermostatDevices)
                {
                    ThermostatDevices = new List<ThermostatDevice>();
                }
                ThermostatDevices.Clear();


                foreach (var thermostat in dynObj.devices.thermostats)
                {
                    ThermostatDevice tsd = new ThermostatDevice();

                    tsd.device_id = thermostat.First.device_id;
                    tsd.name = thermostat.First.name;
                    tsd.where_id = thermostat.First.where_id;
                    tsd.where_name = thermostat.First.where_name;
                    tsd.label = thermostat.First.label;
                    tsd.name_long = thermostat.First.name_long;
                    tsd.structure_id = thermostat.First.structure_id;
                    tsd.humidity = thermostat.First.humidity;
                    tsd.locale = thermostat.First.locale;
                    tsd.temperature_scale = thermostat.First.temperature_scale;
                    tsd.is_using_emergency_heat = thermostat.First.is_using_emergency_heat;
                    tsd.has_fan = thermostat.First.has_fan;
                    tsd.software_version = thermostat.First.software_version;
                    tsd.has_leaf = thermostat.First.has_leaf;
                    tsd.can_heat = thermostat.First.can_heat;
                    tsd.can_cool = thermostat.First.can_cool;
                    tsd.target_temperature_c = thermostat.First.target_temperature_c;
                    tsd.target_temperature_f = thermostat.First.target_temperature_f;
                    tsd.target_temperature_high_c = thermostat.First.target_temperature_high_c;
                    tsd.target_temperature_high_f = thermostat.First.target_temperature_high_f;
                    tsd.target_temperature_low_c = thermostat.First.target_temperature_low_c;
                    tsd.target_temperature_low_f = thermostat.First.target_temperature_low_f;
                    tsd.ambient_temperature_c = thermostat.First.ambient_temperature_c;
                    tsd.ambient_temperature_f = thermostat.First.ambient_temperature_f;
                    tsd.eco_temperature_high_c = thermostat.First.eco_temperature_high_c;
                    tsd.eco_temperature_high_f = thermostat.First.eco_temperature_high_f;
                    tsd.eco_temperature_low_c = thermostat.First.eco_temperature_low_c;
                    tsd.eco_temperature_low_f = thermostat.First.eco_temperature_low_f;
                    tsd.is_locked = thermostat.First.is_locked;
                    tsd.locked_temp_min_c = thermostat.First.locked_temp_min_c;
                    tsd.locked_temp_min_f = thermostat.First.locked_temp_min_f;
                    tsd.locked_temp_max_c = thermostat.First.locked_temp_max_c;
                    tsd.locked_temp_max_f = thermostat.First.locked_temp_max_f;
                    tsd.sunlight_correction_active = thermostat.First.sunlight_correction_active;
                    tsd.sunlight_correction_enabled = thermostat.First.sunlight_correction_enabled;
                    tsd.fan_timer_active = thermostat.First.fan_timer_active;
                    tsd.fan_timer_timeout = thermostat.First.fan_timer_timeout;
                    tsd.fan_timer_duration = thermostat.First.fan_timer_duration;
                    tsd.previous_hvac_mode = thermostat.First.previous_hvac_mode;
                    tsd.hvac_mode = thermostat.First.hvac_mode;
                    tsd.time_to_target = thermostat.First.time_to_target;
                    tsd.time_to_target_training = thermostat.First.time_to_target_training;
                    tsd.is_online = thermostat.First.is_online;
                    tsd.hvac_state = thermostat.First.hvac_state;

                    ThermostatDevices.Add(tsd);
                }


                if (null == Structures)
                {
                    Structures = new List<Structure>();
                }
                Structures.Clear();

                foreach (var structure in dynObj.structures)
                {
                    Structure s = new Structure();

                    s.structure_id = structure.First.structure_id;
                    s.name = structure.First.name;
                    s.away = structure.First.name;

                    for (int i = 0; i < structure.First.thermostats.Count; ++i)
                    {
                        ThermostatDevice t = ThermostatDevices.Where(T => structure.First.thermostats[i] == T.device_id).SingleOrDefault();
                        if (null != t && structure.First.thermostats[i] == t.device_id)
                        {
                            s.thermostats.Add(t);
                        }
                    }

                    foreach (var where in structure.First.wheres)
                    {
                        Where w = new Where();

                        w.where_id = where.First.where_id;
                        w.name = where.First.name;

                        s.wheres.Add(w);
                    }

                    Structures.Add(s);
                }


                foreach (ThermostatDevice tsd in ThermostatDevices)
                {
                    Structure s = Structures.Where(S => tsd.structure_id == S.structure_id).SingleOrDefault();
                    if (null != s && tsd.structure_id == s.structure_id)
                    {
                        tsd.Structure = s;

                        Where w = tsd.Structure.wheres.Where(W => tsd.where_id == W.where_id).SingleOrDefault();
                        if (null != w && tsd.where_id == w.where_id)
                        {
                            tsd.Where = w;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
            }

            return false;
        }


        public string AccessToken(string json)
        {
            try
            {
                dynamic dynObj = JsonConvert.DeserializeObject(json);

                return dynObj.access_token;
            }
            catch (Exception ex)
            {
            }

            return "";
        }


        public string ErrorMessage(string json)
        {
            try
            {
                dynamic dynObj = JsonConvert.DeserializeObject(json);

                return "error: " + dynObj.message;
            }
            catch (Exception ex)
            {
            }

            return "error: ";
        }
    }
}
