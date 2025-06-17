using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetHeadersOfAttendaceSummaryForHrUserDto
    {
        public int WorkingDay { get; set; }
        public int UserWorkingDay { get; set; }
        public int UserAbsentDays { get; set; }
        public int DaysPerWeek { get; set; }
        public int DayOff { get; set; }
        public int Vacation { get; set; }
        public int PersonalVacation { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
        public int DaysOFMonth { get; set; }
    }
}
