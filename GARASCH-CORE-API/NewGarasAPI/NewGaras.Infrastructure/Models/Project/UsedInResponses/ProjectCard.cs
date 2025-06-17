using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class ProjectCard : MiniProjectCard
    {
        [DataMember]
        public string ProjectRevision { get; set; }

        [DataMember]
        public string MaintnanceType { get; set; } //(Maintenance Contract, Emergency Visit, Warrenty, Project In Warrenty Period)


        [DataMember]
        public string ProjectManagerName { get; set; }
        [DataMember]
        public string ProjectManagerImgUrl { get; set; }

        [DataMember]
        public string ProjectStartDate { get; set; }
        [DataMember]
        public string ProjectEndDate { get; set; }
        [DataMember]
        public int RemainTime { get; set; }

        [DataMember]
        public string FinishProjectReportStatus { get; set; }

        [DataMember]
        public decimal OfferPrice { get; set; }
        [DataMember]
        public decimal OfferTax { get; set; }
        [DataMember]
        public decimal OfferExtraCost { get; set; }
        [DataMember]
        public decimal TotalProjectCollectedCost { get; set; }
        [DataMember]
        public string TotalProjectCollectedCostPercentage { get; set; }
        [DataMember]
        public decimal RemainCollection { get; set; }

        [DataMember]
        public int OpenFabOrders { get; set; }
        [DataMember]
        public int TotalFabOrders { get; set; }
        [DataMember]
        public decimal TotalHoursFabOrders { get; set; }
        [DataMember]
        public string FabOrdersPercentage { get; set; }

        [DataMember]
        public int OpenInstallationOrders { get; set; }
        [DataMember]
        public int TotalInstallationOrders { get; set; }
        [DataMember]
        public decimal TotalHoursInstallationOrders { get; set; }
        [DataMember]
        public string InstallationOrdersPercentage { get; set; }
        [DataMember]
        public decimal BOMMaterialCost { get; set; }
        [DataMember]
        public decimal BOMCurrentMaterialCost { get; set; }
        [DataMember]
        public string BOMMaterialCostDate { get; set; }
        [DataMember]
        public decimal BOMReleasedMaterialCost { get; set; }
        [DataMember]
        public decimal ExtraReleasedMaterialCost { get; set; }
        [DataMember]
        public decimal TotalReleasedMaterialCost { get; set; }
        [DataMember]
        public string BOMReleasedPercentage { get; set; }
        [DataMember]
        public string ExtraReleasedPercentage { get; set; }
        [DataMember]
        public string TotalReleasedPercentage { get; set; }

        [DataMember]
        public decimal RemainMaterialPricingTimeCost { get; set; }
        [DataMember]
        public decimal RemainMaterialCurrentCost { get; set; }
        [DataMember]
        public decimal RemainMaterialDiffCost { get; set; }

        [DataMember]
        public string LastBOMMaterialCostModifiedDate { get; set; }
        [DataMember]
        public long OfferId { get; set; }
    }
}
