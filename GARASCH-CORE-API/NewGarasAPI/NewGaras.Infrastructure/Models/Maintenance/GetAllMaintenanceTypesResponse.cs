using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetAllMaintenanceTypesResponse
    {
        public List<VehicleMaintenanceTypeItem> MaintenanceTypeItemsList { get; set; }
        public PaginationHeader PaginationHeader { get; set; }
        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
