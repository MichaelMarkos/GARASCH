using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task
{
    public class TaskWorkFlosGroups
    {
        public List<TaskGroupingByWorkFlow> groupList { get; set; }
        public int CountOfTasks { get; set; }
    }
}
