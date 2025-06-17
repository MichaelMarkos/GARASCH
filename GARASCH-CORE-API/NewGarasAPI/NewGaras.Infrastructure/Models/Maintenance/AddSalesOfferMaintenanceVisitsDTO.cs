using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class AddSalesOfferMaintenanceVisitsDTO
    {
        public long OfferID { get; set; }
        public string PlannedDate { get; set; }
        public string VisitDate { get; set; }
    }
}
