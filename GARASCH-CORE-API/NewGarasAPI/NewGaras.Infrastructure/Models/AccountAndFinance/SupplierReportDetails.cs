using NewGarasAPI.Models.Supplier;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class SupplierReportDetails
    {
        private string supplierName;
        private decimal volume;
        private decimal collected;
        private decimal remain;
        private decimal paidPercent;

        private List<SupplierPODetails> supplierPOsList;
        [DataMember]
        public string SupplierName { get => supplierName; set => supplierName = value; }
        [DataMember]
        public decimal Volume { get => volume; set => volume = value; }
        [DataMember]
        public decimal Collected { get => collected; set => collected = value; }
        [DataMember]
        public decimal Remain { get => remain; set => remain = value; }
        [DataMember]
        public decimal PaidPercent { get => paidPercent; set => paidPercent = value; }
        [DataMember]
        public List<SupplierPODetails> SupplierPOsList { get => supplierPOsList; set => supplierPOsList = value; }
    }
}
