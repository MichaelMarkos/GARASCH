using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.InternalTicket
{
    public class GetSalesOfferDetailsForInternalTicketResponse
    {
        public SalesOfferDetailsForInternalTicket SalesOfferDetails { get; set; }
        public List<SalesOfferProductForInternalTicket> SalesOfferProducts { get; set; }

        public bool Result { get; set; }
        public List<Error> Errors { get; set; }

    }
}
