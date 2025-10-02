using NewGaras.Infrastructure.DTO.LMS;


namespace NewGaras.Infrastructure.Models.LMS
{
    public class AcademicYearViewModel
    {
        public string YearName { get; set; }

        public List<AcademicYearDto> dtolist { get; set; }
    }
}
