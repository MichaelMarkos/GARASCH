using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetClientsFilters
    {
        [FromHeader]
        public string SearchedKey { get; set; }
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]
        public int BranchId { get; set; }
        [FromHeader]
        public bool IncludeOwner { get; set; }
    }
}
