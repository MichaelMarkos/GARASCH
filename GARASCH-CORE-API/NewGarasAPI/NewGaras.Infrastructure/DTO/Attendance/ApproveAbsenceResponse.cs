using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class ApproveAbsenceResponse
    {
        public long RequestId { get; set; }
        public long HrUserId { get; set; }

        public long SecondReportTo { get; set; } = 0;
    }
}
