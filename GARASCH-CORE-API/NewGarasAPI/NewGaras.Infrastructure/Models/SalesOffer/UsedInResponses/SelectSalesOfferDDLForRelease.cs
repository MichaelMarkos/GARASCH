using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.UsedInResponses
{
    public class SelectSalesOfferDDLForRelease
    {
        public long ID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectSerial { get; set; }
        public string OfferSerial { get; set; }
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public long? ClientId { get; set; }
        public string ClientName { get; set; }
        public string ReleaseStatus { get; set; }
        public decimal PercentReleased { get; set; }
    }
}
