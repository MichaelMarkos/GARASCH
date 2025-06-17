using NewGaras.Infrastructure.Models.Maintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle.UsedInResponse
{
    public class VehicleMaintenanceTypes
    {
        public long VehicleId { get; set; }
        public List<VehicleMaintenanceTypeItem> VehicleMaintenanceTypeList { get; set; }
    }
}
