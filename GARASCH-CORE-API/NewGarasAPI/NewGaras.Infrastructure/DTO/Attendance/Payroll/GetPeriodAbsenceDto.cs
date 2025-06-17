using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance.Payroll
{
    public class GetPeriodAbsenceDto
    {
        public long ID { get; set; }
        public long HrUserId { get; set; }
        public long AbsenceTypeID { get; set; }
        public string AbsenceTypeName { get; set; }
        public bool? IsApprovedAbsence { get; set; }
        public long? ApprovedByUserID { get; set; }
        public string ApprovedByUserName { get; set; }

    }
}
