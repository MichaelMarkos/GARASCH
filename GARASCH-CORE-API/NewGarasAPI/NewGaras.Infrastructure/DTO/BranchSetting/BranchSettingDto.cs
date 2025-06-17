using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.BranchSetting
{
    public class BranchSettingDto
    {
        public bool AllowOverTimeInWeekends { get; set; }

        public bool AllowDelayingDeduction { get; set; }

        public bool AllowAutomaticOvertime { get; set; }

        public int? branchId { get; set; }
        public bool Active { get; set; }
        public bool? ApplySettingdForAll { get; set; }

        public int PayrollFrom {  get; set; }

        public int PayrollTo { get; set;}

    }
}
