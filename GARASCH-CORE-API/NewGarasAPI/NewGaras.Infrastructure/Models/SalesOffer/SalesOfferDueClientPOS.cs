using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferDueClientPOS
    {
        public long OfferID { get; set; }
        public decimal finalOfferPrice { get; set; }
        public string projectName { get; set; }
        public string CreationDate { get; set; }
        public string OfferType { get; set; }
        public string StoreName { get; set; }
        public List<SalesOfferProductPOS> ProductList { get; set; }
    }
}
