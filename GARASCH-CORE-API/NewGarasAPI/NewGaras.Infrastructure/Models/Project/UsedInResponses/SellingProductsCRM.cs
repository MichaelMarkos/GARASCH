using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class SellingProductsCRM
    {
        [DataMember]
        public long ProductId { get; set; }
        [DataMember]
        public int? ProductCategoryId { get; set; }

        [DataMember]
        public string ProductName { get; set; }

        [DataMember]
        public decimal TotalSoldPrice { get; set; }

        [DataMember]
        public int SoldCount { get; set; }
    }
}
