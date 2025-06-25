using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class AddNewSalesOfferForInternalTicketRequest
    {
        public SalesOfferInternalTicket SalesOffer { get; set; }
        public List<GetSalesOfferProduct> SalesOfferProductList { get; set; }
        public List<GetTax> SalesOfferTaxList { get; set; }
    }
}
