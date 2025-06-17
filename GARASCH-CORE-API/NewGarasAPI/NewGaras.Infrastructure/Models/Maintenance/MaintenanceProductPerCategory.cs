using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceProductPerCategory
    {
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int NoOFMaintenance { get; set; }
    }
}
