using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetMyReportsDetailsCRMFilters
    {
        [FromHeader]
        public int Month { get; set; }
        [FromHeader]
        public int Year { get; set; }
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]
        public long ClientId { get; set; }
        [FromHeader]
        public long ReportCreator { get; set; }
        [FromHeader]
        public int BranchId { get; set;}
        [FromHeader]
        public string ClientStatus { get; set; }
    }
}
