using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle.filters
{
    public class TransferVehicleFilters
    {
        [FromHeader]
        public long? ClientVehicleId { get; set; }
        [FromHeader]
        public long? ClientId { get; set; }
    }
}
