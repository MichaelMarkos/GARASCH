using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.AssetDepreciation
{
    public class GetAssetDepreciationDTO
    {
        public long ID { get; set; }
        public long DepreciationTypeID { get; set; }
        public string DepreciationTypeName { get; set; }
        public decimal CostOfAsset { get; set; }
        public string YearOfPurchase { get; set; }
        public decimal? ResidualValue { get; set; }
        public int ExpectedLifespanPerMonth { get; set; }
        public long? ProductionUOMID { get; set; }
        public string ProductionUOMName { get; set; }
        public int? ProductionUOMCount { get; set; }
        public decimal DepreciationRate { get; set; }
        public decimal RealCost { get; set; }
        public long CreatedByID { get; set; }
        public string CreatedByName { get; set; }
        public string DateOfCreation { get; set; }
        public long ModifiedByID { get; set; }
        public string ModifiedByName { get; set; }
        public string DateOfModification { get; set; }
    }
}
