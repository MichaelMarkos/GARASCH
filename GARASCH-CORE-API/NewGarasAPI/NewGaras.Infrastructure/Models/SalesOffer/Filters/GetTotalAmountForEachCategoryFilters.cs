using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.Filters
{
    public class GetTotalAmountForEachCategoryFilters
    {
        [FromHeader]
        public string from { get; set; }
        [FromHeader]
        public string to { get; set; }

    }
}
