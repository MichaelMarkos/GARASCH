
using System.ComponentModel.DataAnnotations;

namespace NewGaras.Infrastructure.Models.LMS
{

    public class SummuryViewModel
    {
        public List<Term> Term { get; set; }
    }

    public class Term
    {
        public string? Termname { get; set; }
        public int? TermId { get; set; }
        public DateTime? from { get; set; }
        public DateTime? to { get; set; }
        public List<Leveling> levels { get; set; }

    }
    public class Leveling
    {
        public string? levelname { get; set; }
        public int? levelId { get; set; }
        public int? programmId { get; set; }
        public List<Depart> depart { get; set; }

    }
    public class Depart
    {
        public string? departname { get; set; }
        public int? deptId { get; set; }
        public List<Spical>? Spical { get; set; }

    }
    public class Spical
    {
        public string spicalname { get; set; }
        public int spicalId { get; set; }
        public List<Sub>? sub { get; set; }
    }
    public class Sub
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Objective { get; set; }

        public string? ImagePath { get; set; }
        //public string? CertificateTempImg { get; set; }
        public bool Active { get; set; }
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
        //public List<Depart> depart { get; set; }

        public int RemineNumberLectures { get; set; }
        public int TotalNumberLectures { get; set; }

        public int SpecialdeptId { get; set; }
        public int AcademiclevelId { get; set; }
        public int AcademicYearId { get; set; }
    }
}

