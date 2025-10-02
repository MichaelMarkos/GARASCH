
using System.ComponentModel.DataAnnotations;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class FilterTabledDto
    {
        public int CompetitionDayId { get; set; }
        public int CompetitionId { get; set; }
        public int TypeId { get; set; }
        public string Name { get; set; }
        public string? NameCompetitionDay { get; set; }
        public string? NameCompetition { get; set; }
        public string? CompetitionName { get; set; }
        [Required]
        public string? From { get; set; }

        [Required]
        public string? To { get; set; }



        public int? NumberOfStudents { get; set; }
        public int? NumberOfAttendce { get; set; }
        public int? hallid { get; set; }
        public string? hallName { get; set; }
        public string specialDeptName { get; set; }
        public string DeptName { get; set; }
        public string levelName { get; set; }
        public string DoctorName { get; set; }
        public string? lecturerName { get; set; }
        public string? ProgramName { get; set; }
        public bool? AttendanceFlag { get; set; }
        public decimal? FromScore { get; set; }    //new
        public decimal? UserScore { get; set; }    //new
    }
    public class FilterTabledDto2
    {
        public int CompetitionDayId { get; set; }
        public int CompetitionId { get; set; }
        public string Name { get; set; }
        public string? NameCompetition { get; set; }
        [Required]
        public DateTime? From { get; set; }

        [Required]
        public DateTime? To { get; set; }
        public int? NumberOfStudents { get; set; }
        public int? NumberOfAttendce { get; set; }
        public int? hallid { get; set; }
        public string? hallName { get; set; }
        public string Location { get; set; }
        public string specialDeptName { get; set; }
        public string levelName { get; set; }
        public string NameOfDoctor { get; set; }
        public DateTime? date { get; set; }
        public string? ImagePath { get; set; }

    }
    public class FilterTabledDto3
    {
        public DateTime? datetime { get; set; }
        public List<FilterTabledDto2> Allcompetitionday { get; set; }
    }


}
