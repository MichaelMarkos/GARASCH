using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Attendance
{
    public class AddHolidayToBranchAttendanceModel
    {
        public long vacationDayID { get; set; }

        public int branchID { get; set; }
    }
}
