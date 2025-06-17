using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class AddMaintenanceReportExpensesRequest
    {
        public long? MaintenanceReportExpensesId{get; set;}
        public long MaintenanceReportId {get; set;}
        public decimal? Amount{get; set;}
        public int? CurrencyId{get; set;}
        public string Comment{get; set;}
        public bool? Approve{get; set;}
        public IFormFile File{get; set;}
        public long? ExpensesTypeId{get; set;}
    }
}
