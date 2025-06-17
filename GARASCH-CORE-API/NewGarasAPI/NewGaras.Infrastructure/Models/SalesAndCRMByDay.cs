namespace NewGaras.Infrastructure.Models
{
    public class SalesAndCRMByDay
    {
        public string DayDate { get; set; }
        public List<CrmSalesClientReport> CRMReportsList { get; set; }
        public List<CrmSalesClientReport> SalesReportsList { get; set; }
    }
}