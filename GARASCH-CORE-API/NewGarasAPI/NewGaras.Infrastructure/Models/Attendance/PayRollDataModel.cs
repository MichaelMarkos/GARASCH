using NewGaras.Infrastructure.DTO.Salary;
using NewGaras.Infrastructure.DTO.Salary.SalaryTax;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.SalaryAllownce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Attendance
{
    public class PayRollDataModel
    {
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string DepartmentName { get; set; }
        public string JobtitleName { get; set; }
        public string FromDate { get; set; }

        public string ToDate { get; set; }

        public int WorkingDays { get; set; }
        
        public decimal WorkingHourRate { get; set; }

        public string BranchName { get; set; }
        public decimal WorkingHours {  get; set; }
        public string Date {  get; set; }

        public decimal OverTimeHours { get; set; }

        public decimal OverTimeRate { get; set; }

        public decimal DeductionRate { get; set; }
        public decimal BasicRateTotal { get; set; }

        public decimal OverTimeTotal { get; set; }

        public decimal DeductionTotal { get; set; }
        public decimal DelayHours { get; set; }

        public decimal TotalGrossAmount { get; set; }
        public GetSalaryDto SalaryDetais { get; set; }

        public List<EditSalaryAllownce> SalaryAllownces { get; set; }

        public List<PayslipTaxModel> Taxes { get; set; }
        public List<PayslipLeaveModel> LeaveTypes { get; set; }

        public decimal TotalDeductions { get; set; }

        public decimal TotalNetAmount { get; set; }
        public string CurrencyName { get; set; }

        public string Status { get; set; }

        public string Paymethod { get; set; }

        public decimal WorkingDaysNum { get; set; }
        public decimal WeekEndDaysNum { get; set; }
        public decimal HoliyDaysNum { get; set; }
        public decimal HolidayHours { get; set; }
        public decimal HolidayHourRate { get; set; }

        public decimal HoliDaysHoursTotal { get; set; }

        public decimal VacationHours { get; set; }
        public decimal VacationHourRate { get; set; }

        public decimal VacationHoursTotal { get; set; }
        public decimal VacationDaysNum { get; set; }
    }
}
