

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitinForUserAndAdmin
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Objective { get; set; }

        public string? ImagePath { get; set; }
        public bool Active { get; set; }

        public int? Days { get; set; }

        public int? StudyingHours { get; set; }
        public int? Accreditedhours { get; set; }                                   //new
        public string? RequiedofSubject { get; set; }                                   //new
        public int SubjectScore { get; set; }                                    //new

        public string? Code { get; set; }

        public string? CreationBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public string? ApplicationUserId { get; set; }
        public int remainingNumber { get; set; }
        public decimal? TotalScoreStudent { get; set; }                    //درجة الطالب في المادة
        public string? Status { get; set; }                                   //new
        public int? Capacity { get; set; }                                    //new
        public int? TotalStudents { get; set; }                                    //new
        public string LevelName { get; set; }                                   //new
        public string? programName { get; set; }                                   //new
        public int LevelId { get; set; }                                   //new
        public int? programId { get; set; }                                   //new
        public int AcademicYearId { get; set; }
        public string AcademicYearName { get; set; }
        public string? SpecialdeptName { get; set; }
        public int SpecialdeptId { get; set; }
        public string? deptName { get; set; }
        public List<DoctorslistDto>? doctorslistDtos { get; set; }

    }
}
