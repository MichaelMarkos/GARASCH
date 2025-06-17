using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.SalesOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class AddNewSalesOfferWithReleaseForPOSRequest
    {
        public GetSalesOfferPOS SalesOffer { get; set; }
        public List<GetSalesOfferProduct> SalesOfferProductList { get; set; }
        public List<GetTax> SalesOfferTaxList { get; set; }
    }
}
