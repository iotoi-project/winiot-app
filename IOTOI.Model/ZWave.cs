using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOTOI.Model.ZWave
{
    public class ZWaveNode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int Id { get; set; }
        public uint HomeId { get; set; }
        public string Label { get; set; }
        public string Manufacturer { get; set; }
        public string Product { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public bool Value { get; set; }
    }
}
