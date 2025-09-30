

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class FinishCompetitionList
    {
        public List<ProgramList> programLists { get; set; }

    }
    public class ProgramList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal GpaForProgram { get; set; }
        public List<YearList> yearList { get; set; }
        public int? NumOfYears { get; set; }

    }
    public class YearList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal GpaForYear { get; set; }
        public List<TermList> termLists { get; set; }
        public int? NumOfTerms { get; set; }

    }
    public class TermList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal GpaForTerm { get; set; }
        public List<FinishCompetitionVM> FinishCompetitionList { get; set; }
        public int? NumOfCompetitions { get; set; }

    }
    public class FinishCompetitionVM
    {
        public int competitionId { get; set; }
        public string? Name { get; set; }
        public string? ImagePath { get; set; }
        public int SpecialDeptId { get; set; }
        public string? SpecialDeptName { get; set; }
        public string? DeptName { get; set; }

        public int LevelId { get; set; }
        public string? LevelName { get; set; }
        public int? ProgramId { get; set; }

        public string? ProgramName { get; set; }
        public List<string>? doctorsName { get; set; }
        public string? statusOfStudent { get; set; }
        public string? statusOfSubject { get; set; }
        public decimal? Gpa { get; set; }
        public string? GeneralGrade { get; set; }
        public string? Grade { get; set; }
        public int? capacityofCompetition { get; set; } = null;

        public int? StudentsNum { get; set; } = null;
        public int? DelayRequestNum { get; set; } = null;
        public int? WithdrawRequestNum { get; set; } = null;
        public int? PendingNum { get; set; } = null;




        //public int Id { get; set; }
        //public string Name { get; set; }

        //public string? ImagePath { get; set; }
        //public List<string> doctorsName { get; set; }
        //public string levelName { get; set; }
        //public string SpecialName { get; set; }
        //public string DeptName { get; set; }
        //public decimal? Gpa { get; set; } = null;
        //public int? capacityofCompetition { get; set; } = null;
        //public int? StudentsNum { get; set; } = null;
        //public string? statusOfSubject { get; set; }
        //public int? DelayRequestNum { get; set; } = null;
        //public int? WithdrawRequestNum { get; set; } = null;
        //public int? PendingNum { get; set; } = null;
    }
}
