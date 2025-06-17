using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetMonthlyAttendanceModel
    {
        public List<GetAttendanceSum> AttendanceSummary { get; set; }

        public int UsersNumbers { get; set; }

        public int WorkingDaysMumber { get; set; }

        public int Holidays { get; set; }
        public int Weekends { get; set; }
        public int DaysOFMonth { get; set; }
    }
}
