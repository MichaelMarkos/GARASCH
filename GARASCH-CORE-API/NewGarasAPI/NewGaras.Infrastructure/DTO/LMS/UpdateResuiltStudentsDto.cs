

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class UpdateResuiltStudentsDto
    {
        public List<UpdateResuiltStudent> UpdateResuiltStudent { get; set; }

    }
    public class UpdateResuiltStudent
    {
        public long userId { get; set; }

        public List<UpdateDegree> UpdateDegree { get; set; }
    }

    public class UpdateDegree
    {
        public int competitionDayId { get; set; }
        public decimal degree { get; set; }

    }
}
