using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class MiniProjectCard
    {
        [DataMember]
        public long ProjectId { get; set; }

        [DataMember]
        public string ProjectSerial { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public string projectStatus { get; set; }

        [DataMember]
        public decimal ProjectExtraModifications { get; set; }

        [DataMember]
        public decimal TotalProjectPrice { get; set; }

        [DataMember]
        public string projectType { get; set; } //(ProjectOrders, RentOrders, MaintenanceOrders, WarrantyOrders)

        [DataMember]
        public string ProjectSalesPersonImgUrl { get; set; }

        [DataMember]
        public string ProjectSalesPersonName { get; set; }

        //public decimal OfferPrice;
        //public decimal OfferTax;
        //public decimal OfferExtraCost;
    }
}
