using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Attendance
{
    public class AddAttendanceModel
    {
        public long? Id { get; set; }
        public long HrUserId { get; set; }

        public DateTime Checkin {  get; set; }

        public DateTime CheckOut { get; set; }

        public DateTime date {  get; set; }
    }
}
