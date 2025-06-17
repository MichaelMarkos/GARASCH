using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class GetSalesOfferPOS
    {
        public long? Id { get; set; }
        public string RentStartDate { get; set; }
        public string RentEndDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public long SalesPersonId { get; set; }
        public int BranchId { get; set; }
        public string ClientApprovalDate { get; set; }
        public string OfferExpirationDate { get; set; }
        public string ProjectStartDate { get; set; }
        public string ProjectEndDate { get; set; }
        public string ReminderDate { get; set; }
        public string SendingOfferDate { get; set; }
        public string CreatedBy { get; set; }
        public long? ParentSalesOfferID { get; set; }
        public string OfferType { get; set; }
        public long? ParentInvoiceID { get; set; }
        public int? InventoryStoreId { get; set; }
        public string Note { get; set; }
        public string TechnicalInfo { get; set; }
        public string ProjectData { get; set; }
        public string FinancialInfo { get; set; }
        public string PricingComment { get; set; }
        public decimal? OfferAmount { get; set; }
        public string Status { get; set; }
        public bool Completed { get; set; }
        public bool? SendingOfferConfirmation { get; set; }
        public string SendingOfferBy { get; set; }
        public string SendingOfferTo { get; set; }
        public string SendingOfferComment { get; set; }
        public bool? ClientApprove { get; set; }
        public string ClientComment { get; set; }
        public int? VersionNumber { get; set; }
        public long? ClientId { get; set; }
        public string ProductType { get; set; }
        public string ProjectName { get; set; }
        public string ProjectLocation { get; set; }
        public string ContactPersonMobile { get; set; }
        public string ContactPersonEmail { get; set; }
        public string ContactPersonName { get; set; }
        public string ContractType { get; set; }
        public bool? ClientNeedsDiscount { get; set; }
        public string RejectionReason { get; set; }
        public bool? NeedsInvoice { get; set; }
        public bool? NeedsExtraCost { get; set; }
        public int? OfferExpirationPeriod { get; set; }
        public decimal? ExtraOrDiscountPriceBySalesPerson { get; set; }
        public decimal? FinalOfferPrice { get; set; }

        public bool? IsHospitality { get; set; }

    }
}
