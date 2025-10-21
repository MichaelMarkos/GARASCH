
namespace NewGaras.Infrastructure.Models.LMS
{
    public class ResuiltStudentsInCompetition
    {
        public long UserId { get; set; }
        public string SerialNum { get; set; }
        public string UserName { get; set; }
        public decimal lecturesDegree { get; set; }
        public List<IdandName>? MissionList { get; set; }
        public List<IdandName>? ResearchList { get; set; }
        public List<IdandName>? QuizList { get; set; }

        public List<IdandName>? MidtermList { get; set; }
        public List<IdandName>? FinalexamList { get; set; }
        public decimal TotalDegree { get; set; }
    }

    public class IdandName
    {
        public int TypeId { get; set; }
        public int? CompetitionDayId { get; set; }
        public string NameOfCometitionDay { get; set; }
        public decimal Degree { get; set; } = 0;
    }

    public class ResuiltStudentOfCompetition
    {
        public long UserId { get; set; }
        public string SerialNum { get; set; }
        public string UserName { get; set; }
        public decimal lecturesDegree { get; set; }
        public List<IdandName>? MissionList { get; set; }
        public List<IdandName>? ResearchList { get; set; }
        public List<IdandName>? QuizList { get; set; }

        public List<IdandName>? MidtermList { get; set; }
        public List<IdandName>? FinalexamList { get; set; }
        public decimal TotalDegree { get; set; }
        public decimal DegreeOfCompetition { get; set; }
        public decimal gpa { get; set; }
        public bool CorrectionDone { get; set; }
    }
}
