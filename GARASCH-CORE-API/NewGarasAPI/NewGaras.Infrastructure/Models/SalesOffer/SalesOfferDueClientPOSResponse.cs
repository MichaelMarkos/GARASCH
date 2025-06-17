using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferDueClientPOSResponse
    {
        public string ClientName { get; set; }
        public List<SalesOfferDueClientPOS> SalesOfferDueClientPOSList { get; set; }
        public string FilePath { get; set; }
    }
}
