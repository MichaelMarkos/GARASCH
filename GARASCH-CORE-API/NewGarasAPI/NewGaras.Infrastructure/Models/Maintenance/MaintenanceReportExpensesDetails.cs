namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceReportExpensesDetails
    {
        public long MaintenanceReportExpensesId { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string FilePath { get; set; }
        public bool Approve { get; set; }
        public long? ExpensesTypeId { get; set; }
        public string ExpensesTypeName { get; set; }
    }
}