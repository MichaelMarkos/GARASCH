using NewGaras.Infrastructure.Models.Maintenance.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class AddSalesOfferProductListForMainenance
    {
        public long offerId { get; set; }
        public long AssginTo { get; set; }

        public List<AddSalesOfferProductForMainenance> productList { get; set; }
    }
}
