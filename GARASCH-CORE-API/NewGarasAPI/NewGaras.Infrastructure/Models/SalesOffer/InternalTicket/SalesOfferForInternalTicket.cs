using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.InternalTicket
{
    public class SalesOfferForInternalTicket
    {
        public List<SalesOfferDetailsForInternalTicket> SalesOfferList { get; set; }
        public int TotalOffersCount { get; set; }
        public decimal TotalOffersPrice { get; set; }
    }
}
