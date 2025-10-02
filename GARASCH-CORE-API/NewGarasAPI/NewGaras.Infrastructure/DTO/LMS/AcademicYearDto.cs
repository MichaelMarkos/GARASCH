

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class AcademicYearDto
    {
        public int? Id { get; set; }
        public int YearId { get; set; }
        public string? YearName { get; set; }
        public string Term { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
