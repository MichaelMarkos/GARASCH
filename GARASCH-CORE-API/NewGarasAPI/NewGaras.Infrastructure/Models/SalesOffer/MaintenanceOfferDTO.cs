using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class MaintenanceOfferDTO
    {
        public long? OfferId { get; set; }
        public string OfferSerial { get; set; }
        public string StartDate { get; set; } = DateTime.Now.ToString();
        public string EndDate { get; set; } = DateTime.Now.ToString();

        public long ClientId { get; set; }
        
        public string ClientName { get; set; }

        public string WorkerOrder {  get; set; }

        public long SalesPersonId { get; set; }
        
        public string SalesPersonName { get; set; }

        public string ContactPersonName { get; set; }

        public string ContactPersonMobile { get; set; }

        public string ContactPersonEmail { get; set; }
        public string ProjectLocation { get; set; }

        public string ProductFabricator { get; set; }

        public string OfferType { get; set; }
        public decimal LocationX { get; set; }
        public decimal LocationY { get; set; }
        public long ProjectId { get; set; }
        public long? MaterialRequestId { get; set; }
        public string MaterialRequestStatus { get; set; }
        public List<MaintenanceOfOffer> Maintenances { get; set; } = [];

        public List<GetSalesOfferProduct> SalesOfferProducts { get; set; } = [];

        public long CreatorID { get; set; }
        public string CreatorName { get; set; }
        public string OfferStatus { get; set; }
        public List<string> VisitsDates { get; set; }
    }
}
