using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle.UsedInResponse
{
    public class VehicleMaintenanceJobOrderHistoryModel
    {
        public long? Id { get; set; }
        public long CurrentMaintenanceTypeId { get; set; }
        public long ClientId { get; set; }
        public string ClientName { get; set; }
        public string VehicleMaintenanceTypeName { get; set; }
        public long CuurentOpenJobOrderId { get; set; }
        public long JobOrderProjectId { get; set; }
        public string JobOrderProjectName { get; set; }
        public long ClientVehicleId { get; set; }
        public int? CurentOpenJobOrderMilage { get; set; }
        public long? NextVisitForId { get; set; }
        public string NextVisitForName { get; set; }
        public int? NextVisitMilage { get; set; }
        public string NextVisitDate { get; set; }
        public string NextVisitComment { get; set; }
        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }
        public string ModifiedBy { get; set; }
    }
}
