using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetAttendanceByDayModel
    {
        public int UsersNumber { get; set; }

        public string Date { get; set; }

        public List<AttendanceByDay> AttendanceList {  get; set; } 
    }
}
