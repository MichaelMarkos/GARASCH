namespace NewGaras.Infrastructure.Models
{
    public class SalesReportLine
    {
        public long? Id { get; set; }
        public long? ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string ClientMobile { get; set; }
        public string ClientEmail { get; set; }
        public string ClientExpirationDate { get; set; }
        public string ClientStatus { get; set; }
        public string ClientClassification { get; set; }
        public string Hint { get; set; }
        public int DailyReportThroughId { get; set; }
        public string DailyReportThroughName { get; set; }
        public string ReportDate { get; set; }
        public float? FromTime { get; set; }
        public float? ToTime { get; set; }
        public string Location { get; set; }
        public string PickLocation { get; set; }
        public string Reason { get; set; }
        public bool IsNew { get; set; }
        public string NewClientAddress { get; set; }
        public string NewClientTel { get; set; }
        public bool IsReviewed { get; set; }
        public long? ReviewedBy { get; set; }
        public string ReviewedByName { get; set; }
        public string NewClientName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonMobile { get; set; }
        public int? ReasonTypeId { get; set; }
        public string ReasonTypeName { get; set; }
        public decimal? CustomerSatisfaction { get; set; }
        public long? RelatedToInventoryItemId { get; set; }
        public long? RelatedToSalesOfferProductId { get; set; }
        public long? RelatedToSalesOfferId { get; set; }
        public string ReminderDate { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public List<SalesReportLineExpense> SalesReportLineExpenses { get; set; }
        public List<long> CRMLinesIds { get; set; }
        public string RelatedToInventoryItemName { get; set; }

        public string RelatedToSalesOfferProductName { get; set; }

        public long? RelatedToProjectProductId { get; set; }
        public string RelatedToProjectProductName { get; set; }

        public string RelatedToSalesOfferSerial { get; set; }
        public string RelatedToSalesOfferName { get; set; }
        public long? RelatedToProjectId { get; set; }
        public string RelatedToProjectSerial { get; set; }
        public string RelatedToProjectName { get; set; }
        public string CreationDate { get; set; }
        public string Comment { get; set; }

        public bool RemindIsClosed { get; set; }
    }
}