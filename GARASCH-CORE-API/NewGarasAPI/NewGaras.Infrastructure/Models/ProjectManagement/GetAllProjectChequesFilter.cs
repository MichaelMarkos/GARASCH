using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectManagement
{
    public class GetAllProjectChequesFilter
    {
        [FromHeader]
        public string Bank {  get; set; }

        [FromHeader]
        public string Branch { get; set; }
        [FromHeader] 
        public string clientName {  get; set; }
        [FromHeader]
        public bool? IsCrossed { get; set; }
        [FromHeader] 
        public string ProjectName { get; set; }  
        [FromHeader]
        public int? cashingStatus { get; set; }
        [FromHeader] 
        public int? month {  get; set; }

        [FromHeader]
        public int? year { get; set; }
        [FromHeader]
        public int pageNumber { get; set; } = 1;
        [FromHeader]
        public int pageSize { get; set; } = 10;

        [FromHeader]
        public long? MaintenanceForID { get; set; }

        [FromHeader]
        public long? MaintenanceOrderID { get; set; }
    }
}
