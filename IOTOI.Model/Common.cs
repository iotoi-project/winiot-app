using IOTOI.Model.ZigBee;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOTOI.Model.Common
{
    public class ProtocolType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }


        public virtual ICollection<ZigBeeEndPoint> EndPoints { get; set; }

    }
}
