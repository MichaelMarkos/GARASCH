using NewGaras.Infrastructure.Models.Task;

namespace NewGaras.Infrastructure.Models
{
    public class MonitoredTasks
    {
        public string TaskName { get; set; }

        public  List<GetTaskMonitor> MonitorData { get; set; }
    }
}