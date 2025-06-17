namespace NewGarasAPI.Models.Account
{
    public class AccumulativePerMonth
    {
        public int month { get; set; }
        public decimal credit { get; set; }
        public decimal Debit { get; set; }
        public decimal Accumulative { get; set; }
    }
}