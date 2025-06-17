using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class UpdateReminderDateVisitOfMaintenanceRequest
    {
        public long VisitOfMaintenanceId { get; set; }
        public string ReminderDate {  get; set; }
        public string ReminderHint { get; set; }
    }
}
