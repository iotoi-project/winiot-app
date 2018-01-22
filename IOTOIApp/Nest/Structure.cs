using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTOIApp.Nest
{
    public class Structure
    {
        public string structure_id { get; set; }
        public string name { get; set; }
        public string away { get; set; }
        public List<ThermostatDevice> thermostats { get; private set; }
        public List<Where> wheres { get; private set; }

        


        public Structure()
        {
            thermostats = new List<ThermostatDevice>();
            wheres = new List<Where>();            
        }
    }


    public class Where
    {
        public string where_id { get; set; }
        public string name { get; set; }
    }
}
