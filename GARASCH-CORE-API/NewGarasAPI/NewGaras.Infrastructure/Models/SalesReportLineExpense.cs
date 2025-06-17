using NewGaras.Infrastructure.Models.Inventory;

namespace NewGaras.Infrastructure.Models
{
    public class SalesReportLineExpense
    {
        public long? Id { set; get; }
        public long DailyReportLineID { set; get; }
        public decimal? Amount { set; get; }
        public string Type { set; get; }
        public int? CurrencyId { set; get; }
        public string CurrencyName { set; get; }
        public string Comment { set; get; }
        public string FilePath { get; set; }

        public AttachmentFile AttachmentObj { set; get; }
    }
}