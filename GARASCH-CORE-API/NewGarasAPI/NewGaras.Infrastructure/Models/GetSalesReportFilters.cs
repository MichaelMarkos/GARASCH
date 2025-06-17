using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetSalesReportFilters
    {
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]

        public int BranchId { get; set; }
        [FromHeader]

        public DateTime? CreationFrom { get; set; }
        [FromHeader]

        public DateTime? CreationTo { get; set; }
        [FromHeader]

        public bool? IsReviewed { get; set; }
        [FromHeader]

        public bool? NotApproved { get; set; }
        [FromHeader]

        public bool? HaveCRM { get; set; }
        [FromHeader]

        public int CurrentPage { get; set; } = 1;
        [FromHeader]

        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]

        public string Status { get; set; }


    }
}
