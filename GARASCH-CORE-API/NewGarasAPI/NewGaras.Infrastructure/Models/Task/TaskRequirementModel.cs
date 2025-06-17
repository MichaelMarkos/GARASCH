using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task
{
    public class TaskRequirementModel
    {
        public long TaskId { get; set; }

        public List<RequirementModel> RequirementList { get; set; }
    }
}
