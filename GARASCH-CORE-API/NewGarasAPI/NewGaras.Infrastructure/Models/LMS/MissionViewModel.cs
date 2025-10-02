
namespace NewGaras.Infrastructure.Models.LMS
{
    public class MissionViewModel
    {
        public string status { get; set; }
        public DateTime? DateOfupload { get; set; }
        public decimal? degree { get; set; }
        public bool? corrected { get; set; }
        public string? ContentCompetitionDay { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? startDate { get; set; }
        public int? NumberOfStudent { get; set; }
        public string? pdfUrl { get; set; }
        public string? comment { get; set; }

    }
}
