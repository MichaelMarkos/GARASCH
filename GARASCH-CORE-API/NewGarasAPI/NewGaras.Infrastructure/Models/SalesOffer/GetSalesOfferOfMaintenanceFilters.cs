using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesOfferOfMaintenanceFilters
    {
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public long clientId { get; set; }
        [FromHeader]
        public long salesPersonId { get; set; }
        [FromHeader]
        public string searchKey { get; set; } // On Client Mobile - Client Contact Person
        [FromHeader]
        public string workerOrder { get; set; } // On Offer Serial Contain
        [FromHeader]
        public string equipmentName { get; set; }
        [FromHeader]
        public string equipmentBrand { get; set; }
        [FromHeader]
        public string vendor { get; set; }
        [FromHeader]
        public DateOnly? DateFrom { get; set; }
        [FromHeader]
        public DateOnly? DateTo { get; set; }
        [FromHeader]
        public string Status { get; set; }
        [FromHeader]
        public string Type { get; set; }
        [FromHeader]
        public string VisitDate { get; set; }

    }
}
