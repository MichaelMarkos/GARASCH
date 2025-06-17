using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class AddVisitsScheduleOfMaintenanceRequest
    {
        public List<VisitsScheduleOfMaintenanceData> VisitsScheduleOfMaintenanceDataList { get; set; }
    }
}
