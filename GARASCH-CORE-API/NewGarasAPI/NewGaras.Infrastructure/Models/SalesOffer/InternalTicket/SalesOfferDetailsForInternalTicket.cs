using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.InternalTicket
{
    public class SalesOfferDetailsForInternalTicket
    {
        public long? Id { get; set; }
        public string OfferSerial { get; set; }
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string ContactPersonName { get; set; }
        public long? ClientId { get; set; }
        public string ClientName { get; set; }

        public long? ParentSalesOfferID { get; set; }
        public string ParentSalesOfferSerial { get; set; }

        public string CreatorName { get; set; }
        public string CreationDate { get; set; }
        public string OfferType { get; set; }
        public string CreationTime { get; set; }
        public decimal? FinalOfferPrice { get; set; }
        public decimal RemainPrice { get; set; }
        public long? TeamId { get; set; }
        public string TeamName { get; set; }


        public List<ChildrenSalesOfferInternalTicket> ChildrenSalesOfferList { get; set; }

    }
}
