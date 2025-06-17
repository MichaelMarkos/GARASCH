using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class RejectClosedSalesOfferData
    {
        public long? SalesOfferId { get; set; }
        public bool DeleteAutomaticJE { get; set; }
        public bool DeleteInvoice {  get; set; }
    }
}
