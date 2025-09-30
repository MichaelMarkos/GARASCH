using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class ResultControlForProgram
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string UserSerialNum { get; set; }

    public int? ProgrammId { get; set; }

    public string ProgrammName { get; set; }

    public int? CompetitionsNums { get; set; }

    public int? DelayNums { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Totalcredithour { get; set; }

    [Column("TotalGPA", TypeName = "decimal(18, 2)")]
    public decimal TotalGpa { get; set; }

    public string GeneralGrade { get; set; }

    public string Grade { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? ModficationDate { get; set; }

    [Column("HrUserID")]
    public long? HrUserId { get; set; }

    public int? AcademicYearId { get; set; }

    public int? CompetitionId { get; set; }

    public int AcademicLevelId { get; set; }

    [ForeignKey("AcademicYearId")]
    [InverseProperty("ResultControlForPrograms")]
    public virtual AcademicYear AcademicYear { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("ResultControlForPrograms")]
    public virtual Competition Competition { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("ResultControlForPrograms")]
    public virtual HrUser HrUser { get; set; }

    [ForeignKey("ProgrammId")]
    [InverseProperty("ResultControlForPrograms")]
    public virtual Programm Programm { get; set; }
}
