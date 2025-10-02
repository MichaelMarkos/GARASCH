

namespace NewGaras.Infrastructure.Models.LMS
{
    public class AllNextMissionAndReaserchVM
    {
        public string competitionName { get; set; }
        public string competitionDayName { get; set; }
        public int competitionDayId { get; set; }
        public int competitionId { get; set; }
        public int typeId { get; set; }
        public string termName { get; set; }
        public string levelName { get; set; }
        public string SpecialDeptName { get; set; }
        public string programName { get; set; }
        public string? ImagePath { get; set; }
        public string status { get; set; }
        public string? ShowFile { get; set; }
        public string? comment { get; set; }
        public string? DateOfupload { get; set; }
        public string? DateCompetitionDay { get; set; }
        public string? EndDateCompetitionDay { get; set; }
        public decimal? degreeOfCompetitionDay { get; set; }
        public decimal? degree { get; set; }
    }
}
