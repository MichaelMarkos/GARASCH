using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Task
{
    public class GetTaskMonitorList
    {
        public string UserName { get; set; }

        public string TaskName { get; set; }

       
        public List<GetTaskMonitor> MonitorList { get; set; }
    }
}
