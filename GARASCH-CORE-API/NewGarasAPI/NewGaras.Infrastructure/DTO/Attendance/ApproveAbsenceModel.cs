using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class ApproveAbsenceModel
    {
        public long RequestId { get; set; }

        public bool Approval { get; set; }

        public string AbsenceRejectCause { get; set; }
    }
}
