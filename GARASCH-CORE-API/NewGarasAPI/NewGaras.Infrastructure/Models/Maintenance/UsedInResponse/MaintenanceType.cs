using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance.UsedInResponse
{
    public class MaintenanceType
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public int VehicleRateId { get; set; }
        public string VehicleRateName { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public int VehiclePriorityLevelId { get; set; }
        public bool isForAllModels { get; set; }
        public VehicleMaintenanceTypeBOM VehicleMaintenanceTypeBOM { get; set; }
        public List<string> VehicleMaintenanceTypeServiceSheduleCategories { get; set; }
        public List<string> VehicleMaintenanceTypeForModelsStrings { get; set; }
        public List<string> VehicleMaintenanceTypeForBrandsStrings { get; set; }
        public int? Milage { get; set; }
    }
}
