
namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Objective { get; set; }

        public string? ImagePath { get; set; }
        public string? CertificateTempImg { get; set; }

        public int? Days { get; set; }

        public int? StudyingHours { get; set; }

        public string? Code { get; set; }

        public string? CreationBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool IsJoined { get; set; }
        public decimal? TotalScore { get; set; }
        public string? TotalScorePercentage { get; set; }
        public decimal? TotalFromScore { get; set; }
        public int? RemainDay { get; set; }
        public int? RankNo { get; set; }
        public int? TotalRank { get; set; }
        public int? CountOfCompetitorJoined { get; set; }
        public bool? ShowAnswers { get; set; }
        public bool? ShowRanks { get; set; }
        public bool? ShowScores { get; set; }
        public bool? ShowCertificate { get; set; }
        public bool? IsOwner { get; set; }
        public bool? IsMemberAdmin { get; set; }


        public ICollection<CompetitionDayDTO> CompetitionDayDTO { get; set; }

        public bool CanPrint { get; set; }
        public decimal? SolvedPercent { get; set; }
        public bool Visable { get; set; } = true;


    }
}
