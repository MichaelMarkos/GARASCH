namespace NewGaras.Infrastructure.Models
{
    public class SalesReportExpense
    {
        public long? Id { set; get; }
        public long DailyReportID { set; get; }
        public decimal Amount { set; get; }
        public string Type { set; get; }
        public int CurrencyId { set; get; }
        public int CurrencyName { set; get; }
    }
}