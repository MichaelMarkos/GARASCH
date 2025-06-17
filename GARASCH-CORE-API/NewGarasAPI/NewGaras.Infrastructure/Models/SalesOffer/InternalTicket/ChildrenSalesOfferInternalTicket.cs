using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.InternalTicket
{
    public class ChildrenSalesOfferInternalTicket
    {
        public long SalesOfferId { get; set; }
        public string SalesOfferSerial { get; set; }
        public decimal? TotalPrice { get; set; }
        public string CreationDate { get; set; }
    }
}
