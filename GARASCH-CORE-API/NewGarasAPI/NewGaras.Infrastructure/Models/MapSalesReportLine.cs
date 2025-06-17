namespace NewGaras.Infrastructure.Models
{
    public class MapSalesReportLine
    {
        public long LineId { get; set; }
        public long ReportId { get; set; }
        public string ReportDate { get; set; }
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string SalesPersonImage { get; set; }
        public string Longiude { get; set; }
        public string Latitude { get; set; }
        public string LocationName { get; set; }
    }
}