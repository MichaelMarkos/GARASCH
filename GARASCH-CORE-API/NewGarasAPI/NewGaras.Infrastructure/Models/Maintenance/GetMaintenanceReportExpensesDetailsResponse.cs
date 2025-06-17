using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetMaintenanceReportExpensesDetailsResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public List<MaintenanceReportExpensesDetails> MaintenanceReportExpensesDetails { get; set; }
    }
}
