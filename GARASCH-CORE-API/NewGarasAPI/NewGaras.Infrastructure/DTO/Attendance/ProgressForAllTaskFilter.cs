using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class ProgressForAllTaskFilter
    {
        [FromHeader]
        public long projectId { get; set; }
        [FromHeader]
        public bool GroupByTask { get; set; }

        [FromHeader]
        public bool GroupByUser { get; set; }
        [FromHeader]
        public bool GroupByDate { get; set; }
        [FromHeader]
        public bool IsProjectInvoice { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int PageSize { get; set; } = 10;
        [FromHeader]
        public bool GroupByJobTitle { get; set; }
    }
}
