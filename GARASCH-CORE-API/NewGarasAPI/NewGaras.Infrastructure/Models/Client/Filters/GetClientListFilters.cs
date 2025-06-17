using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client.Filters
{
    public class GetClientListFilters
    {
        [FromHeader]
        public int? NeedApproval { get; set; }
        [FromHeader]
        public int? BranchId { get; set; }
        [FromHeader]
        public long? SalesPersonId { get; set;}
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public string ClientType { get; set; }
    }
}
