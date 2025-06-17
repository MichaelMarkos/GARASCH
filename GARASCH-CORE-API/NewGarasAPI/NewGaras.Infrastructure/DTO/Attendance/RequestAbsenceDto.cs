using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class RequestAbsenceDto
    {
        public DateTime From {  get; set; }
        public DateTime To {  get; set; }
        public int AbsenceTypeId { get; set; }

        public long HrUserId { get; set; }

        public string AbsenceCause { get; set; }

        public long FirstApprovedBy { get; set; }
        public long SecondApprovedBy { get; set; }
    }
}
