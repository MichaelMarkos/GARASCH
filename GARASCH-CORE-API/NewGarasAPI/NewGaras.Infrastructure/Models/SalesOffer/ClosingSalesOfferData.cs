using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class ClosingSalesOfferData
    {
        public long? SalesOfferId { get; set; }
        public string CreatedBy { get; set; }
        public string SalesOfferIdString { get; set; }
    }
}
