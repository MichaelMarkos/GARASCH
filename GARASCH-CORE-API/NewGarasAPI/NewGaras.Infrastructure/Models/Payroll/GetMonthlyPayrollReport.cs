using NewGaras.Infrastructure.DTO.Attendance.Payroll;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Payroll
{
    public class GetMonthlyPayrollReport
    {
        public int? BranchID { get; set; }
        public int? DepartmentID { get; set; }
        public int? PaymentMethodID { get; set; }
        public long? HrUserID { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalNumberOfActiveUsers{ get; set; }
        public int NumberOfHrUser { get; set; }
        public int  WorkingDays { get; set; }
        public int Vacation { get; set; }
        public int DaysOff { get; set; }
        public int DaysOFMonth { get; set; }

        public string PayrollReportPath { get; set; }
        public List<GetMonthlyPayrollReportDto> PayrollMonthList { get; set; }

    }
}
