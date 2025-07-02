using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.AssetDepreciation
{
    public class GetAssetDepreciationFilters
    {
        [FromHeader]
        public string YearOfPurchase { get; set; }
        [FromHeader]
        public long? AccountID { get; set; }
    }
}
