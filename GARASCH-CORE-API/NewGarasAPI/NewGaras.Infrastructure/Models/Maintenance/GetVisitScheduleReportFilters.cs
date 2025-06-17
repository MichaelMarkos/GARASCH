using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetVisitScheduleReportFilters
    {
        [FromHeader]
        public string VisitStatus { get; set; }
        [FromHeader]
        public DateTime? From { get; set; }
        [FromHeader]
        public DateTime? To { get; set; }
        [FromHeader]
        public long? ClientId { get; set; }
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public double? Months { get; set; }
    }
}
