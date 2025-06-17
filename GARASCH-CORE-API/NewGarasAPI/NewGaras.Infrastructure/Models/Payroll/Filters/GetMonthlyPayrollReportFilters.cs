using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Payroll.Filters
{
    public class GetMonthlyPayrollReportFilters
    {
        [FromHeader]
        public int BranchID { get; set; }
        [FromHeader]
        public int? DepartmentID { get; set; }
        [FromHeader]
        public int? PaymentMethodID { get; set; }
        [FromHeader]
        public long? HrUserID { get; set; }
        [FromHeader]
        public int Month { get; set; }
        [FromHeader]
        public int Year { get; set; }

        [FromHeader]
        public bool DownloadExcel { get; set; }
    }
}
