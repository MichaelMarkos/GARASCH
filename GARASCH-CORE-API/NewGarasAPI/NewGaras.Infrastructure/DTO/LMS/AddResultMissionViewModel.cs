

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class AddResultMissionViewModel
    {
        public long UserIdForStudent { get; set; }
        public decimal Degree { get; set; }
        public int competitionDayId { get; set; }
        public int competitionId { get; set; }
        public string? comment { get; set; }

    }
    public class AddResultMidTermViewModel
    {
        public long UserIdForStudent { get; set; }
        public decimal Degree { get; set; }
        public int competitionDayId { get; set; }
        public int competitionId { get; set; }
        public string? comment { get; set; }

    }
}
