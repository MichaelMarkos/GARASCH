using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle.filters
{
    public class GetClientVehiclesDataResponseFilters
    {
        [FromHeader]
        public long? ClientId { get; set; }
        [FromHeader]
        public long? VehicleId { get; set; }
        [FromHeader]
        public string SearchedKey { get; set; }
        [FromHeader]
        public int? Month { get; set; }
        [FromHeader]
        public int? Year { get; set; }
        [FromHeader]
        public string VehicleCreationFrom { get; set; }
        [FromHeader]
        public string VehicleCreationTo { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
    }
}
