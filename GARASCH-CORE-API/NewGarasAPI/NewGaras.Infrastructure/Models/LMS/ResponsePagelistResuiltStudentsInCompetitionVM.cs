

namespace NewGaras.Infrastructure.Models.LMS
{
    public class ResponsePagelistResuiltStudentsInCompetitionVM
    {
        public List<ResuiltStudentsInCompetition> data { get; set; }
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public PaginationHeader PaginationHeader { get; set; }
        public bool CorrectionDone { get; set; }

    }
}
