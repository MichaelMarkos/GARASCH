namespace NewGaras.Infrastructure.Models
{
    public class AchievedTarget
    {
        public decimal TargetAmount { get; set; }
        public decimal TargetExtraCostAmount { get; set; }
        public decimal TargetAmountLastYear { get; set; }
        public decimal TargetExtraCostsAmountLastYear { get; set; }
        public decimal Achieved {  get; set; }
        public decimal AchievedLastYear { get; set; }
        public string Currency { get; set; }
        public string AchievedPercentage { get; set; }
        public string AchievedPercentageLastYear { get; set; }
        public string AchievedState { get; set; } //(Up, Down
    }
}