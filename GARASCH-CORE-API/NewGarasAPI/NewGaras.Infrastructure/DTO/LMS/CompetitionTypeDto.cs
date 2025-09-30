using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionTypeDto
    {
        public int CompetitionId { get; set; }
        public string? Name { get; set; }
        public string? ImagePath { get; set; }
        public bool Active { get; set; }
        public int? StudyingHours { get; set; }
        public int? Accreditedhours { get; set; }                                   //new
        public int SubjectScore { get; set; }                                    //new

        public string? Code { get; set; }

        public string? CreationBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public string? ApplicationUserId { get; set; }
        public decimal? TotalScoreStudent { get; set; }
        public int? NumbersOfStudents { get; set; }                                    //new
        public int? NumbersOfAdmin { get; set; }                                    //new
        public int? NumbersOfDoctor { get; set; }                                    //new

        public int? NumbersOfRequestDelay { get; set; }                                    //new
        public int? NumbersOfRejectDelay { get; set; }                                    //new
        public int? NumbersOfRequestWithdraw { get; set; }                                    //new
        public int? NumbersOfRejectWithdraw { get; set; }                                    //new
        public int? NumbersOfPending { get; set; }                                    //new




        public int SpecialDept { get; set; }                                    //new    // NEW TO FILTER
        public int LevelId { get; set; }                                    //new        // NEW TO FILTER
        public string? LevelName { get; set; }
        public string? AcademicYearName { get; set; }
        public string? ProgramName { get; set; }
        public string? LectureName { get; set; }

        public List<TypeList>? typeLists { get; set; }
    }
}
