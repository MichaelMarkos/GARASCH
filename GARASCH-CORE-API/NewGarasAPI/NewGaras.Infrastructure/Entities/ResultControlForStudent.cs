using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class ResultControlForStudent
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

    public int AcademicYearId { get; set; }

    [Required]
    public string AcademicYearName { get; set; }

    public DateTime TermFrom { get; set; }

    public DateTime TermTo { get; set; }

    public int? ProgrammId { get; set; }

    public string ProgrammName { get; set; }

    public int? CompetitionsNums { get; set; }

    public int? DelayNums { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalCreditHour { get; set; }

    [Column("TotalGPA", TypeName = "decimal(18, 2)")]
    public decimal TotalGpa { get; set; }

    public string GeneralGrade { get; set; }

    public string Grade { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? ModficationDate { get; set; }

    [Column("HrUserID")]
    public long? HrUserId { get; set; }

    public int? CompetitionId { get; set; }

    public int AcademiclevelId { get; set; }

    [ForeignKey("AcademicYearId")]
    [InverseProperty("ResultControlForStudents")]
    public virtual AcademicYear AcademicYear { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("ResultControlForStudents")]
    public virtual Competition Competition { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("ResultControlForStudents")]
    public virtual HrUser HrUser { get; set; }

    [ForeignKey("ProgrammId")]
    [InverseProperty("ResultControlForStudents")]
    public virtual Programm Programm { get; set; }
}
