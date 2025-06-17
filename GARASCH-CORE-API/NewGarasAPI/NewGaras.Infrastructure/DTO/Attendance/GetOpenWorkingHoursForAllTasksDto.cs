using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetOpenWorkingHoursForAllTasksDto
    {
        public long Id { get; set; }

        public long? TaskId { get; set; }

        public string TaskName { get; set; }

        public string Date {  get; set; }
        public decimal? ProgressRate {  get; set; }

        public TimeOnly CheckIn { get; set; }

        public string LastResponseTime { get; set; }
    }
}
