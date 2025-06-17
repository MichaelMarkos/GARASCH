using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance.Payroll
{
    public class GetPeriodAttendance
    {
        public long ID { get; set; }
        public long HrUserId { get; set; }
        //--------------working Hours-------------
        public string checkIn { get; set; }
        public string CheckOut { get; set; }
        public string TotalHours { get; set; }
        public string OverTime { get; set; }
        public string Delay { get; set; }
    }
}
