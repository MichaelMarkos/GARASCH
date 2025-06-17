using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class CRMReportsDetailsFilters
    {
        [FromHeader]
        public DateTime StartDate { get; set; }
        [FromHeader]
        public DateTime EndDate { get; set; }
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]
        public long ReportCreator { get; set; }
        [FromHeader]
        public string ContactType { get; set;}
        [FromHeader]
        public string ThroughName { get; set;}
        [FromHeader]
        public long CRMUserId { get; set;}
        [FromHeader]
        public int BranchId { get; set;}
        [FromHeader]
        public long ClientId { get; set;}
        [FromHeader]
        public long CRMId { get; set;}
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public DateTime ReminderDate { get; set; }
    }
}
