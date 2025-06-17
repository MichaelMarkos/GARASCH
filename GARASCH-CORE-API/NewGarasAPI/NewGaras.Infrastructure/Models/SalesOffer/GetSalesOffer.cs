using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesOffer
    {
        public long? Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Note { get; set; }
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string Status { get; set; }
        public bool Completed { get; set; }
        public string TechnicalInfo { get; set; }
        public string ProjectData { get; set; }
        public long? ProjectID { get; set; }
        public string FinancialInfo { get; set; }
        public string PricingComment { get; set; }
        public decimal? OfferAmount { get; set; }
        public bool? SendingOfferConfirmation { get; set; }
        public string SendingOfferDate { get; set; }
        public string SendingOfferBy { get; set; }
        public string SendingOfferTo { get; set; }
        public string SendingOfferComment { get; set; }
        public bool? ClientApprove { get; set; }
        public string ClientComment { get; set; }
        public int? VersionNumber { get; set; }
        public string ClientApprovalDate { get; set; }
        public long? ClientId { get; set; }
        public string ClientName { get; set; }
        public string ProductType { get; set; }
        public string ProjectName { get; set; }
        public string ProjectLocation { get; set; }
        public string ContactPersonMobile { get; set; }
        public string ContactPersonEmail { get; set; }
        public string ContactPersonName { get; set; }
        public string ProjectStartDate { get; set; }
        public string ProjectEndDate { get; set; }
        public int BranchId { get; set; }
        public string OfferType { get; set; }
        public string ContractType { get; set; }
        public string OfferSerial { get; set; }
        public bool? ClientNeedsDiscount { get; set; }
        public string RejectionReason { get; set; }
        public bool? NeedsInvoice { get; set; }
        public bool? NeedsExtraCost { get; set; }
        public string OfferExpirationDate { get; set; }
        public int? OfferExpirationPeriod { get; set; }
        public decimal? ExtraOrDiscountPriceBySalesPerson { get; set; }
        public decimal? FinalOfferPrice { get; set; }
        public decimal? TotalExtraCostAmount { get; set; }
        public decimal? TotalDiscountAmount { get; set; }
        public decimal? TotalTaxAmount { get; set; }
        public string ReminderDate { get; set; }
        public string CreationDate { get; set; }
        public string CreationTime { get; set; }
        public decimal GrossProfitPercentage { get; set; }
        public decimal GrossProfitValue { get; set; }
        public string ReleaseStatus { get; set; }
        public decimal PercentReleased { get; set; }
        public int? SalesOfferProductsCount { get; set; } //For Rent Offer
        public decimal? SalesOfferProductsQuantity { get; set; } //For Rent Offer
        public long? HasJournalEntryId { get; set; } //For Rent Offer
        public List<GetInvoiceData> SalesOfferInvoices { get; set; }
        public long? ParentSalesOfferID { get; set; }
        public string ParentSalesOfferSerial { get; set; }
        
        public string CreatorName { get; set; }

        public string ModifierName { get; set; }
        public List<ChildrenSalesOFfer> ChildrenSalesOfferList { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        public string ModifiedDate { get; set; }
        public bool? Active { get; set; }
        public long? ClientVehicleId { get; set; }
        public string ClientVehicleName { get; set; }
        public string VehicleChassisNumber { get; set; }
        public string VehicleModelName { get; set; }
        public string ClientTaxCard { get; set; }
        public List<GetClientAddress> ClientAddress { get; set; }
        public string ProjectSerial { get; set; }
        public string ProjectStatus { get; set; }
        public string BranchName { get; set; }
        public decimal ProjectExtraModifications { get; set; }
        public string RentStartDate { get; set; } //For Rent Offer 
        public string RentEndDate { get; set; } //For Rent Offer
        public long? ParentInvoiceID { get; set; }
        public long? LinkedSalesOfferId { get; set; }
        public string MaintenanceType { get; set; }
        /* //For Maintenance Offer
        public decimal ReleasedPercentage { get; set; }
         //For Maintenance Offer
        
        

        



        // Modified by michael markos 2022-10-25
        
        */     // Modified by michael markos 2022-11-28

    }

    public class ChildrenSalesOFfer
    {
        public long SalesOfferId { get; set; }
        public string SalesOfferSerial { get; set; }
        public string CreationDate { get; set; }
        public decimal? FinalPrice { get; set; }
    }


}