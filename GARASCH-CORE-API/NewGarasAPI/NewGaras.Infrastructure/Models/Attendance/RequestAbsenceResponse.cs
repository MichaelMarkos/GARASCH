using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Attendance
{
    public class RequestAbsenceResponse
    {
        public long Id { get; set; }
        public long FirstReportToId { get; set; }
        public long SecondReportToId { get; set; }
    }
}
