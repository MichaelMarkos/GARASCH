namespace NewGaras.Infrastructure.Models
{
    public class CrmSalesClientReport
    {
        public long ID { get; set; }
        public long ClientID { get; set; }
        public string ClientName { get; set; }
        public string ClientAddress { get; set; }
        public string ClientStatus { get; set; }
        public string ClientCalssification { get; set; }
        public string SalesName { get; set; }
        public string ReportDate { get; set; }
        public string CreationDate { get; set; }
        public int ThroughID { get; set; }
        public string ThroughName { get; set; }
        public double FromTime { get; set; }
        public double ToTime { get; set; }
        public string Location { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public string SalesReportCreator { get; set; }

        public int LinesCount { get; set; }
        public long? LineId { get; set; }

        public long CRMUserId { get; set; }
        public string CRMUserName { get; set; }
        public bool CRMIsRecieved { get; set; }
        public string CRMContactName { get; set; }
        public string CRMContactMethod { get; set; }

        public bool IsNew { get; set; }
        public string NewClientName { get; set; }
        public string NewClientAddress { get; set; }
        public string NewClientTele {  get; set; }

        public string ContactPersonName { get; set; }
        public string ContactPersonMobile {  get; set; }
        public string ContactType { get; set; }

        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FilePath { get; set; }

        public string ReviewComment { get; set; }
        public long? ReviewedBy { get; set; }
        public string ReviewedByName { get; set; }
        public string ReviewDate { get; set; }
        public bool? IsReviewed { get; set; }
        public double? Review {  get; set; }

        public string ReminderDate { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public long? RelatedToInventoryItemId { get; set; }
        public string RelatedToInventoryItemName { get; set; }

        public long? RelatedToSalesOfferProductId { get; set; }
        public string RelatedToSalesOfferProductName { get; set; }

        public long? RelatedToProjectProductId { get; set; }
        public string RelatedToProjectProductName { get; set; }

        public long? RelatedToSalesOfferId { get; set; }
        public string RelatedToSalesOfferSerial {  get; set; }
        public string RelatedToSalesOfferName { get; set; }
        public long? RelatedToProjectId { get; set; }
        public string RelatedToProjectSerial { get; set; }
        public string RelatedToProjectName { get; set; }
        public string Hint { get; set; }
        public string ClientSalesPersonName { get; set; }
        public int? ClientClassificationId { get; set; }
        public List<SalesReportLineExpense> SalesReportLineExpenses { get; set; }

        public int? CRMReportReasonID { get; set; }
        public string CRMReportReasonName { get; set; }
    }
}