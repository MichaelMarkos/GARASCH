

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class NoticeDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Thetopic { get; set; }
        public string? Filepath { get; set; }
        public IFormFile? Image { get; set; }
        public DateTime? Date { get; set; }

        public int? SpecialdeptId { get; set; }
        public int? AcademicYearId { get; set; }
        public int? CompetitionId { get; set; }
        public bool? NewsOrAlertsFlag { get; set; }
        public int TypeId { get; set; }

    }
}
