using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTOIApp.Nest
{
    public class ThermostatDevice
    {
        public string device_id { get; set; }
        public string name { get; set; }
        public string where_id { get; set; }
        public string where_name { get; set; }
        public string label { get; set; }
        public string name_long { get; set; }
        public string structure_id { get; set; }
        public string humidity { get; set; }
        public string locale { get; set; }
        public string temperature_scale { get; set; }
        public bool is_using_emergency_heat { get; set; }
        public bool has_fan { get; set; }
        public string software_version { get; set; }
        public bool has_leaf { get; set; }
        public bool can_heat { get; set; }        
        public bool can_cool { get; set; }
        public double target_temperature_c { get; set; }
        public double target_temperature_f { get; set; }
        public double target_temperature_high_c { get; set; }
        public double target_temperature_high_f { get; set; }
        public double target_temperature_low_c { get; set; }
        public double target_temperature_low_f { get; set; }
        public double ambient_temperature_c { get; set; }
        public double ambient_temperature_f { get; set; }
        public double eco_temperature_high_c { get; set; }
        public double eco_temperature_high_f { get; set; }
        public double eco_temperature_low_c { get; set; }
        public double eco_temperature_low_f { get; set; }
        public bool is_locked { get; set; }
        public double locked_temp_min_c { get; set; }
        public double locked_temp_min_f { get; set; }
        public double locked_temp_max_c { get; set; }
        public double locked_temp_max_f { get; set; }
        public bool sunlight_correction_active { get; set; }
        public bool sunlight_correction_enabled { get; set; }        
        public bool fan_timer_active { get; set; }
        public string fan_timer_timeout { get; set; }
        public int fan_timer_duration { get; set; }
        public string previous_hvac_mode { get; set; }
        public string hvac_mode { get; set; }
        public string time_to_target { get; set; }
        public string time_to_target_training { get; set; }        
        public bool is_online { get; set; }
        public string hvac_state { get; set; }

        public Structure Structure { get; set; }
        public Where Where { get; set; }


        public void Duplicate(ThermostatDevice td)
        {   
                td.device_id = this.device_id;
                td.name = this.name;
                td.where_id = this.where_id;
                td.where_name = this.where_name;
                td.label = this.label;
                td.name_long = this.name_long;
                td.structure_id = this.structure_id;
                td.humidity = this.humidity;
                td.locale = this.locale;
                td.temperature_scale = this.temperature_scale;
                td.is_using_emergency_heat = this.is_using_emergency_heat;
                td.has_fan = this.has_fan;
                td.software_version = this.software_version;
                td.has_leaf = this.has_leaf;
                td.can_heat = this.can_heat;
                td.can_cool = this.can_cool;
                td.target_temperature_c = this.target_temperature_c;
                td.target_temperature_f = this.target_temperature_f;
                td.target_temperature_high_c = this.target_temperature_high_c;
                td.target_temperature_high_f = this.target_temperature_high_f;
                td.target_temperature_low_c = this.target_temperature_low_c;
                td.target_temperature_low_f = this.target_temperature_low_f;
                td.ambient_temperature_c = this.ambient_temperature_c;
                td.ambient_temperature_f = this.ambient_temperature_f;
                td.eco_temperature_high_c = this.eco_temperature_high_c;
                td.eco_temperature_high_f = this.eco_temperature_high_f;
                td.eco_temperature_low_c = this.eco_temperature_low_c;
                td.eco_temperature_low_f = this.eco_temperature_low_f;
                td.is_locked = this.is_locked;
                td.locked_temp_min_c = this.locked_temp_min_c;
                td.locked_temp_min_f = this.locked_temp_min_f;
                td.locked_temp_max_c = this.locked_temp_max_c;
                td.locked_temp_max_f = this.locked_temp_max_f;
                td.sunlight_correction_active = this.sunlight_correction_active;
                td.sunlight_correction_enabled = this.sunlight_correction_enabled;
                td.fan_timer_active = this.fan_timer_active;
                td.fan_timer_timeout = this.fan_timer_timeout;
                td.fan_timer_duration = this.fan_timer_duration;
                td.previous_hvac_mode = this.previous_hvac_mode;
                td.hvac_mode = this.hvac_mode;
                td.time_to_target = this.time_to_target;
                td.time_to_target_training = this.time_to_target_training;
                td.is_online = this.is_online;
                td.hvac_state = this.hvac_state;

                td.Structure = this.Structure;
                td.Where = this.Where;
        }
    }
}
