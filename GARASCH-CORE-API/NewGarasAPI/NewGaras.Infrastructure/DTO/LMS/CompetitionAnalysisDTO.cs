

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionAnalysisDTO
    {
        public decimal? CountOfSubscribers { get; set; }
        public string? PercentMaleSubscribers { get; set; }
        public string? PercentFemaleSubscribers { get; set; }
        public string? PercentOtherSubscribers { get; set; }

        public int? CountOfMaleSubscribers { get; set; }
        public int? CountOfFemaleSubscribers { get; set; }
        public int? CountOfOtherSubscribers { get; set; }
        public int CountOfSubscribersLessThan20 { get; set; }
        public int CountOfSubscribersBetween20To30 { get; set; }
        public int CountOfSubscribersBetween30To40 { get; set; }
        public int CountOfSubscribersBetween40To50 { get; set; }
        public int CountOfSubscribersBetween50To60 { get; set; }
        public int CountOfSubscribersAbove60 { get; set; }
        public int CountOfSubscribersWithoutAge { get; set; }
        public int CountOfChurchSubscribers { get; set; }

        public List<ChurchPercent> ChurchPercent { get; set; }
    }

    public class ChurchPercent
    {
        public string ChurchName { get; set; }
        public string Percent { get; set; }
        public decimal PercentPerDecimal { get; set; }
    }
}
