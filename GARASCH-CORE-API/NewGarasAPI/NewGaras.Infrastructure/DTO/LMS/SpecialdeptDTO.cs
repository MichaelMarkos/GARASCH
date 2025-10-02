

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class SpecialdeptDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int deptartmentId { get; set; }
        public string Namedept { get; set; }
    }


    public class SpecialdeptDTO2
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int deptartmentId { get; set; }
        public string? dept { get; set; }
    }
}
