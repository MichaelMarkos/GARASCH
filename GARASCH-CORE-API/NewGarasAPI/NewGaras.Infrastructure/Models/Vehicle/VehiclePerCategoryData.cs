using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle
{
    public class VehiclePerCategoryData
    {
        public long? ID { get; set; }
        public long? ParentID { get; set; }
        public string ItemName { get; set; }
        public bool Active { get; set; }
        public bool? HasChild { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
