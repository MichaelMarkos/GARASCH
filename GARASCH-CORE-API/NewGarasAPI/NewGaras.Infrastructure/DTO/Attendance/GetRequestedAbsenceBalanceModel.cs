using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetRequestedAbsenceBalanceModel
    {
        public long RequestId { get; set; }
        public long HrUserId { get; set; }

        public string UserName { get; set; }
        public string UserImg { get; set; }

        public int AbsenceTypeId { get; set; }

        public string AbsenceName { get; set;}

        public string AbsenceCause { get; set; }

        public string WarningMessage { get; set; }
        public string AbsenceRejectionCause { get; set; }

        public decimal BalancePerMonth { get; set; }

        public int Used {  get; set; }
        public int Remain {  get; set; }
        public string RequestDateFrom {  get; set; }
        public string RequestDateTo {  get; set; }
        public bool? Approval {  get; set; }
        public string ApprovedBy {  get; set; }
        public int Balance {  get; set; }

    }
}
