namespace NewGaras.Infrastructure.Models
{
    public class SalesReport
    {
        public long? Id { set; get; }
        public long UserId { set; get; }
        public string UserName { set; get; }
        public string ReportDate { set; get; }
        public string Status { set; get; }
        public string Note { set; get; }
        public string ReviewComment { set; get; }
        public long? ReviewedBy { set; get; }
        public string ReviewedByName { set; get; }
        public string ReviewDate { set; get; }
        public bool IsReviewed { set; get; }
        public double? Review { set; get; }
        public string CRMReportPercent { set; get; }
        public int LinesCount { set; get; }
        public int CrmCount { set; get; }
        public int ClientsCount { set; get; }
        public int NotApprovedClientsCount { set; get; }
        public int WaitingApprovalClientsCount { set; get; }
    }
}