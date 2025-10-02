

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class AcademiclevelDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public double? MinHours { get; set; }
        public double? MaxHours { get; set; }
        public string? ProgramName { get; set; }
        public int? ProgramId { get; set; }
    }
}
