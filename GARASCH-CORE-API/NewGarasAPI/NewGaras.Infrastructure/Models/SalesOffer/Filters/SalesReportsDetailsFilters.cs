using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.Filters
{
    public class SalesReportsDetailsFilters
    {
        [FromHeader]
        public string StartDate { get; set; }
        [FromHeader]
        public string EndDate { get; set; }
        [FromHeader]
        public long? SalesPersonId { get; set; }
        [FromHeader]
        public long? ReportCreator { get; set; }
        [FromHeader]
        public int? BranchId { get; set; }
        [FromHeader]
        public int? ClientId { get; set; }
        [FromHeader]
        public string ThroughName { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public string ReminderDate { get; set; }
    }
}
