using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class TopSellingProductsCategoryGrouped
    {
        [DataMember]
        public int? CategoryId { get; set; }

        [DataMember]
        public string CategoryName { get; set; }

        [DataMember]
        public decimal TotalDealsCount { get; set; }

        [DataMember]
        public decimal TotalDealsPrice { get; set; }

        [DataMember]
        public List<SellingProductsCRM> TopSellingProductsList { get; set; }
    }

    public class salesofferGrouping
    {
        public long? InventoryItemId { get; set; }
        public string? InventoryItemName { get; set; }
    }
}
