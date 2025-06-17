using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task.Filters
{
    public class GetTasksListReportFilters
    {
        [FromHeader]
        public long? ProjectID { get; set; }
        [FromHeader]
        public int? Priorty { get; set; }
        [FromHeader]
        public long? UserID { get; set; }
        [FromHeader]
        public string Status { get; set; }
        [FromHeader]
        public string DateFrom { get; set; }
        [FromHeader]
        public string DateTo { get; set; }
        [FromHeader]
        public int? TaskTypeID { get; set; }
        [FromHeader]
        public string TaskCategory { get; set; }
        [FromHeader]
        public decimal? Budget { get; set; }
    }
}
