

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class SubjectUpdateDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Objective { get; set; }

        public IFormFile? Image { get; set; }
        public bool Active { get; set; }

        public int? Days { get; set; }

        public int? StudyingHours { get; set; }
        public int? Accreditedhours { get; set; }                                   //new
        public string? RequiedofSubject { get; set; }                                   //new
        public int SubjectScore { get; set; }                                    //new

        public string? Code { get; set; }

        public string? CreationBy { get; set; }
        public DateTime? CreationDate { get; set; }


        public double? ApprovedHours { get; set; }                       //new
        public double? GPAScale { get; set; }                       //new


        public List<SubjectRelationshipList>? SubjectRelationshipLists { get; set; }                                                //New


    }



    public class SubjectRelationshipList
    {

        public int MainSubjectId { get; set; }

        public int SubSubjectId { get; set; }

        public string Status { get; set; }


    }
}
