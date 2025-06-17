using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance.Payroll
{
    public class GetMonthlyPayrollReportDto
    {
        public long PayrollID { get; set; }
        public long HrUserID { get; set; }
        public string UserFullName { get; set; }
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set;}
        public long TeamID { get; set; }
        public string TeamName { get; set; }
        public int JobtitleID { get; set; }
        public string JobTitleName { get; set; }
        public int TotalWorkingdays { get; set; }
        public decimal TotalOverTimeHours { get; set; }
        public decimal TotalWorkingHours { get; set; }
        public decimal TotalDelayHours { get; set; }
        public decimal PaidVacationHours { get; set; }
        public decimal HolidayHours { get; set; }
        public int VacationDaysNumber { get; set; }
        public int TotalAbsenceDays { get; set; }
        public decimal Allowances { get; set; }
        public decimal Tax { get; set; }
        public decimal Insurance { get; set; }
        public decimal OtherDeductions { get; set; }
        public decimal OtherIncome { get; set; }
        public int? PaymentMethodID { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal? TotalGross { get; set; }
        public decimal? TotalNet { get; set; }
        public bool? IsPaid { get; set; }

    }
}
