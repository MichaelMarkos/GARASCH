using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class ResultControl
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string UserSerialNum { get; set; }

    public int YearId { get; set; }

    [Required]
    public string YearName { get; set; }

    public int AcademiclevelId { get; set; }

    [Required]
    public string AcademiclevelName { get; set; }

    public int AcademicYearId { get; set; }

    [Required]
    public string AcademicYearName { get; set; }

    public int DepartId { get; set; }

    [Required]
    public string DepartName { get; set; }

    public int SpecialdeptId { get; set; }

    [Required]
    public string SpecialdeptName { get; set; }

    public int CompetitionId { get; set; }

    [Required]
    public string CompetitionName { get; set; }

    public int? ProgrammId { get; set; }

    [Column("ProgrammNAME")]
    public string ProgrammName { get; set; }

    public DateTime TermFrom { get; set; }

    public DateTime TermTo { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Accreditedhours { get; set; }

    [Required]
    public string StudentStatus { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SubjectScore { get; set; }

    [Column("lecturesDegree", TypeName = "decimal(18, 2)")]
    public decimal LecturesDegree { get; set; }

    [Column("lecturesDegreeFrom", TypeName = "decimal(18, 2)")]
    public decimal LecturesDegreeFrom { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MissionDegree { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MissionDegreeFrom { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ResearchDegree { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ResearchDegreeFrom { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal QuizDegree { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal QuizDegreeFrom { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MidtermDegree { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MidtermDegreeFrom { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal FinalDegree { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal FinalDegreeFrom { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalDegreeBefore { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalDegreeAfter { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Bonus { get; set; }

    public string ReasonOfBonus { get; set; }

    public bool IsClosed { get; set; }

    [Column("GPA", TypeName = "decimal(18, 2)")]
    public decimal Gpa { get; set; }

    public string GeneralGrade { get; set; }

    public string Grade { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? ModficationDate { get; set; }

    [Column("HrUserID")]
    public long? HrUserId { get; set; }

    [ForeignKey("AcademicYearId")]
    [InverseProperty("ResultControls")]
    public virtual AcademicYear AcademicYear { get; set; }

    [ForeignKey("AcademiclevelId")]
    [InverseProperty("ResultControls")]
    public virtual Academiclevel Academiclevel { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("ResultControls")]
    public virtual Competition Competition { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("ResultControls")]
    public virtual HrUser HrUser { get; set; }

    [ForeignKey("ProgrammId")]
    [InverseProperty("ResultControls")]
    public virtual Programm Programm { get; set; }

    [ForeignKey("SpecialdeptId")]
    [InverseProperty("ResultControls")]
    public virtual Specialdept Specialdept { get; set; }
}
