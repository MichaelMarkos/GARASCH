using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetTaskMonitorByTaskGroup
    {
        public List<GetTaskMonitorByTask> MonitorsData {  get; set; }
        public int UsersCount { get; set; }
        public int TaskCount { get; set; }
    }
}
