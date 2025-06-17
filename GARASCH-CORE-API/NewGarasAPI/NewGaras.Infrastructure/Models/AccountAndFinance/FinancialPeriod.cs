namespace NewGarasAPI.Models.AccountAndFinance
{
    public class FinancialPeriod
    {
        public int ID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Description { get; set; }
        public bool IsCurrent { get; set; }
        public bool IsClosed { get; set; }
    }
}