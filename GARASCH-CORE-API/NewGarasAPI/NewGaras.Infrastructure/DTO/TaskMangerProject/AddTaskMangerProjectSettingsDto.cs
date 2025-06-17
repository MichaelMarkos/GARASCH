using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskMangerProject
{
    public class AddTaskMangerProjectSettingsDto
    {
        public long ProjectID { get; set; }
        public bool? AllowProjectChatting { get; set; }
        public bool? AllowTaskScreenMonitoring { get; set; }
        public decimal? BillingFactor { get; set; }
        public bool? Billable { get; set; }
        public int? BillingTypeID { get; set; }
        public bool? TimeTracking { get; set; }
        public bool? MoveBySequenceTask { get; set; }
        public bool? MoveBySequenceType { get; set; }
        public bool? UnitRateService { get; set; }
    }
}
