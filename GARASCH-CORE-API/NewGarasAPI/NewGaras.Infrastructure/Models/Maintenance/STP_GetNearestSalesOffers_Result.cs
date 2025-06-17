using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public partial class STP_GetNearestSalesOffers_Result
    {
        public long SalesOfferId { get; set; }
        public double Distance { get; set; }
    }
}
