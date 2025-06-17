namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class TargetLast5yearsData
    {
        public int ID { get; set; }
        public int Year { get; set; }
        public decimal PlannedTarget { get; set; }
        public int CurrencyID { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal AchievedTarget { get; set; }
        public bool Active { get; set; }
        public bool CanEdit { get; set; }
    }
}