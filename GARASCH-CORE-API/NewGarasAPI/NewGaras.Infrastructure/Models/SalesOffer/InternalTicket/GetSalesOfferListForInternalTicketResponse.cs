using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.InternalTicket
{
    public class GetSalesOfferListForInternalTicketResponse
    {
        public SalesOfferForInternalTicket SalesOfferList { get; set; }

        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public PaginationHeader PaginationHeader { get; set; }
    }
}
