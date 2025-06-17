using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetAttendanceSum
    {
        //public long Id { get; set; }

        public DateOnly Date { get; set;}
        //public int BranchId { get; set; }
        //public TimeOnly From { get; set; }
        //public TimeOnly To { get; set; }
        //public decimal TotalWorkingHours { get; set; }
        //public decimal TotalOverTimeHours { get; set;}
        //public decimal TotalDelayingHours { get; set;}
        //public decimal OverTimeCost { get; set;}
        //public decimal DeductionCost { get; set;}
        public string DayIs {  get; set; }

        public string WeekDayName { get; set; }
        public int AttendanceNum { get; set; }
        public decimal AttendancePercentage { get; set;}

        public int AbsenceNum { get; set; }
        public long? VacationDayId { get; set; }
        public bool? IsWorkingHoursApplied { get; set; }
        public bool? IsPaid { get; set; }

        public int VacationRequestsNum { get; set; }
        public decimal VacationRequestsPercentage { get; set; }
        public List<string> VacationRequestsUsers { get; set; }
    }
}
