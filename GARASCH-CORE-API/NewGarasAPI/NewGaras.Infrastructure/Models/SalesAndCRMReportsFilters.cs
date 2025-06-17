using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class SalesAndCRMReportsFilters
    {
        [FromHeader]
        public DateTime? StartDate { get; set; }
        [FromHeader]
        public int BeforeDays { get; set;}
        [FromHeader]
        public string ClientStatus { get; set; }
        [FromHeader]
        public DateTime? EndDate { get; set;}
        [FromHeader]
        public long SalesPersonId { get; set; }
        [FromHeader]
        public long ReportCreator { get; set; }
        [FromHeader]
        public string ThroughName { get; set;}
        [FromHeader]
        public int BranchId { get; set;}
        [FromHeader]
        public long ClientId { get; set;}
        [FromHeader]
        public DateTime ReminderDate { get; set;}
    }
}
