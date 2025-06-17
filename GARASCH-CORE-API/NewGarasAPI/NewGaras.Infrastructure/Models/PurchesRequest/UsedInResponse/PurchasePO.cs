using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse
{
    public class PurchasePO
    {
        public string IDEnc {  get; set; }
        public long ID { get; set; }
        public string RequestDate { get; set; }
        public string CreationDate { get; set; }
        public string SupplierName { get; set; }
        public decimal? TotalEstimatedCost { get; set; }
        public string Status { get; set; }
        public string TechApprovalStatus { get; set; }
        public string FinalApprovalStatus { get; set; }
        public string ApprovalStatus { get; set; }
        public long? POTypeID { get; set; }
        public string POTypeName { get; set; }
        public bool? SentToSupplier { get; set; }
        public long? AssignedPurchasingPersonID { get; set; }
        public long CreatedByID { get; set; }
        public string CreatedByName { get; set; }
        public bool HasInvoice { get; set; }
        public decimal? TotalInvoice { get; set; }
        public decimal? TotalInvoicePrice { get; set; }
        public long? POInvoiceId { get; set; }
        public int? NoOfRejected { get; set; }
        public decimal? POCollection { get; set; }
        public long? FirstRejectedOffer { get; set; }
    }
}
