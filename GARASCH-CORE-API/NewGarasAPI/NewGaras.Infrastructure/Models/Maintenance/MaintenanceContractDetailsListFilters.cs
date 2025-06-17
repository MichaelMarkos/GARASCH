using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceContractDetailsListFilters
    {
        [FromHeader]
        public string ContractStatus { get; set; }

        [FromHeader]
        public string ContractType { get; set; }

        [FromHeader]
        public bool Late {  get; set; }

        [FromHeader]
        public int WeekNum { get; set; }
    }
}
