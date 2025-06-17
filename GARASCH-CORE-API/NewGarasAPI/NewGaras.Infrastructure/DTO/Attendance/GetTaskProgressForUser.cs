using NewGaras.Infrastructure.DTO.TaskProgress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetTaskProgressForUser
    {
        public long Id { get; set; }

        public string UserImage { get; set; }

        public string UserName { get; set; }

        public long HrUserId { get; set; }

        public decimal? progressrate { get; set; }

        public string progressnote { get; set; }

        public long? ApprovedById { get; set; }

        public string ApprovedByImage { get; set; }
        public string ApprovedByName { get; set; }
        public string Date { get; set; }
        public decimal? TotalHours { get; set; }

        public bool? WorkingHoursApproval {  get; set; }

        public bool FirstNotApprovedProgress { get; set; }
        public List<GetTaskRequirementDto> TaskRequirementList { get; set; }
        public long CreatedBy { get; set; }
        public string CreatorName { get; set; }
        public string UserImgPath { get; set; }
    }
}
