using NewGaras.Infrastructure.Models.Maintenance.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class EditSalesOfferProductListForMainenance
    {
        public long offerId { get; set; }
        public List<EditSalesOfferProductForMainenance> productList { get; set; }
    }
}
