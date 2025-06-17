using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetTaskMonitorByTask
    {
        public string TaskName { get; set; }
        public List<MonitoredUsers> MonitoredUsersList { get; set; }
    }
}
