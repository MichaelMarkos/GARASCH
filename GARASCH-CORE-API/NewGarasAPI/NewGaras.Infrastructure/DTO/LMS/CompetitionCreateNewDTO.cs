
using System.ComponentModel.DataAnnotations;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionCreateNewDTO
    {
        public int? Id { get; set; }
        public int? SubjectId { get; set; }
        public int? AcademiclevelId { get; set; }
        public int? AcademicYearId { get; set; }
        public int SpecialdeptId { get; set; }
        public int? deptId { get; set; }
        public int? CompetitionId { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Objective { get; set; }

        public string? ImagePath { get; set; }
        public IFormFile? Image { get; set; }
        //public string? CertificateTempImg { get; set; }
        public bool Active { get; set; } = true;
        //public bool? ShowAnswers  { get; set; }
        //public bool? ShowRanks  { get; set; }
        //public bool? ShowScores { get; set; }
        //public bool? ShowCertificate { get; set; }

        //[Required]
        public int? Days { get; set; }

        public int? StudyingHours { get; set; }
        public int? Accreditedhours { get; set; }                                   //new
        public string? RequiedofSubject { get; set; }                                   //new
        public int SubjectScore { get; set; }                                    //new

        public string? Code { get; set; }

        [MaxLength(100)]
        public string? CreationBy { get; set; }
        public DateTime? CreationDate { get; set; }
        // public CompetitionCreateNewDTOlist NewDto { get; set; }
        //public decimal? SolvedPercent { get; set; }

        public string? Status { get; set; } = "Open";                                 //new
        public int? Capacity { get; set; }                                    //new
        public int? ProgrammId { get; set; }                                  //New
        public string? ProgramName { get; set; }                                  //New


        public string? AcademiclevelName { get; set; }
        public string? AcademicYearName { get; set; }
        public string? SpecialdeptName { get; set; }
        public string? deptName { get; set; }
        public bool MoreSubjectAtTimeFlag { get; set; } = false;                                //new
        public double? ApprovedHours { get; set; }                       //new
        public double? GPAScale { get; set; }                       //new
        public List<DoctorslistDto>? doctorslistDtos { get; set; }
        public List<SubjectsRequried>? subjectsListRequried { get; set; }
        public List<SubjectsReject>? subjectsListReject { get; set; }
    }
}
