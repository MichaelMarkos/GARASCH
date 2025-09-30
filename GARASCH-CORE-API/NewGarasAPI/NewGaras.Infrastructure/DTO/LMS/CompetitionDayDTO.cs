

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionDayDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public int? DayNumber { get; set; }

        public int? CompetitionId { get; set; }

        public DateTime? Date { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string? CompetitionURL { get; set; }
        public string? BaseCompetitionURL { get; set; }
        public string? AnswerURL { get; set; }

        public string? SpreadSheetId { get; set; }
        public string? SheetName { get; set; }
        public string? NameEntryId { get; set; }

        public string? MobileEntryId { get; set; }

        public string? ChurchEntryId { get; set; }

        public string? UserEntryId { get; set; }
        public int? CountOfCompetitorSolved { get; set; }
        public int? CountOfCompetitorJoined { get; set; }
        public int? CompetitionDayResourceId { get; set; }
        public decimal? score { get; set; }
        public decimal? FromScore { get; set; }
        public int? RankNO { get; set; }
        public int? CountOfCompetitorJoinedPerDay { get; set; }
        public string? ContentCompetitionDay { get; set; }

    }
}
