﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.AssetDepreciation
{
    public class AddAssetDepreciationDTO
    {
        public long DepreciationTypeId { get; set; }
        public long AccountID { get; set; }
        public decimal CostOfAssets { get; set; }
        public string YearOfPurchase { get; set; }
        public decimal? ResidualValue { get; set; }
        public int ExpectedLifespanPerMonth { get; set; }
        public long? ProductionUOMID { get; set; }
        public int? ProductionUOMCount { get; set; }
        public decimal DepreciationRate { get; set; }
        public decimal RealCost { get; set; }
    }
}
