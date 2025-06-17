using NewGaras.Infrastructure.Models.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetTaskMonitorData
    {
        public string UserName { get; set; }
        public string UserImg { get; set; }

        public List<MonitoredTasks> MonitoredTasks { get; set; }
    }
}
