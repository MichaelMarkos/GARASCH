using NewGaras.Infrastructure.Models.Task;

namespace NewGaras.Infrastructure.Models
{
    public class MonitoredUsers
    {
        public string UserName { get; set; }
        public string UserImg { get; set; }
        public List<GetTaskMonitor> MonitorData { get; set; }
    }
}