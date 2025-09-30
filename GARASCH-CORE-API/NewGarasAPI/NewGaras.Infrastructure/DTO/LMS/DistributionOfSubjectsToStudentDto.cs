

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class DistributionOfSubjectsToStudentDto
    {
        public List<DistributionOfSubjectsToStudent> subscribelist { get; set; }
        public List<DistributionOfSubjectsToStudent> competitionSameLevellist { get; set; }
        public List<DistributionOfSubjectsToStudent> competitionDifferentLevellist { get; set; }

    }

    public class DistributionOfSubjectsToStudent
    {
        public int competitionId { get; set; }
        public string? Name { get; set; }
        public string? ImagePath { get; set; }
        public int SpecialDeptId { get; set; }
        public int AcademicYearId { get; set; }
        public string? AcademicYearName { get; set; }
        public string? SpecialDeptName { get; set; }
        public string? DeptName { get; set; }

        public int LevelId { get; set; }
        public string? LevelName { get; set; }
        public int? ProgramId { get; set; }

        public string? ProgramName { get; set; }
        public List<DoctorsDetals>? doctorslist { get; set; }
        public string? statusOfStudent { get; set; }
        public string? statusOfSubject { get; set; }
        public decimal? Gpa { get; set; }

        public int? StudyingHours { get; set; }
        public int? Accreditedhours { get; set; }
        public int SubjectScore { get; set; }
        public int remainingNumber { get; set; }


    }
    public class DoctorsDetals
    {
        public long doctorId { get; set; }
        public string? doctorName { get; set; }
        public string? image { get; set; }
    }
}
