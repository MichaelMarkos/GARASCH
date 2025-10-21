

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class FilterEnrollmentStatusDto
    {
        public long studentId { get; set; }
        public string studentName { get; set; }
        public List<subjectlist> subjectlists { get; set; }
    }


    public class subjectlist
    {
        public int subjectId { get; set; }

        public string subjectName { get; set; }
        public int comptitionUserID { get; set; }

    }
}
