using NewGarasAPI.Models.TaskManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task
{
    public class TaskGroupingByWorkFlow
    {
        public long ProjectWorkFlowID { get; set; }
        public string WorkFlowName { get; set;}
        public List<GetTaskIndex> TaskList { get; set; }
    }
}
