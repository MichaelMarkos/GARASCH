

using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class NoticesDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Thetopic { get; set; }
        public string? Filepath { get; set; }
        public string? FullName { get; set; }
        public string? ImageOfUser { get; set; }
        // public DateTime Date { get; set; }
        public string? Date { get; set; } = DateTime.Now.ToString("yyyy,MM,dd");
        public string? CreationBy { get; set; }
        public int? SpecialdeptId { get; set; }
        public int? AcademicYearId { get; set; }
        public int? CompetitionId { get; set; }
        public string? CompetitionName { get; set; }
        public string? SpecialdeptName { get; set; }
        public string? AcademicYearName { get; set; }
        public bool? NewsOrAlertsFlag { get; set; }
        public string receiverTypeName { get; set; }
    }
}
