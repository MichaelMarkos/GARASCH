using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class CompetitionDay
{
    [Key]
    public int Id { get; set; }

    public int? DayNumber { get; set; }

    public int CompetitionId { get; set; }

    public DateTime? Date { get; set; }

    public DateTime From { get; set; }

    public DateTime To { get; set; }

    [Column("CompetitionURL")]
    public string CompetitionUrl { get; set; }

    public long? CreationBy { get; set; }

    public DateTime? CreationDate { get; set; }

    public bool Active { get; set; }

    public string UserEntryId { get; set; }

    public string ChurchEntryId { get; set; }

    public string MobileEntryId { get; set; }

    public string NameEntryId { get; set; }

    public string ContentCompetitionDay { get; set; }

    [Column("AnswerURL")]
    public string AnswerUrl { get; set; }

    public string SheetName { get; set; }

    public string SpreadSheetId { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? FromScore { get; set; }

    public int? HallId { get; set; }

    public int TypeId { get; set; }

    public string Name { get; set; }

    [Column("lecturerId")]
    public long? LecturerId { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("CompetitionDays")]
    public virtual Competition Competition { get; set; }

    [InverseProperty("CompetitionDay")]
    public virtual ICollection<CompetitionDayResource> CompetitionDayResources { get; set; } = new List<CompetitionDayResource>();

    [InverseProperty("CompetitionDay")]
    public virtual ICollection<CompetitionDayUser> CompetitionDayUsers { get; set; } = new List<CompetitionDayUser>();

    [ForeignKey("HallId")]
    [InverseProperty("CompetitionDays")]
    public virtual Hall Hall { get; set; }

    [ForeignKey("LecturerId")]
    [InverseProperty("CompetitionDays")]
    public virtual HrUser Lecturer { get; set; }

    [ForeignKey("TypeId")]
    [InverseProperty("CompetitionDays")]
    public virtual Type Type { get; set; }

    [InverseProperty("CompetitionDay")]
    public virtual ICollection<UploadFilebyStudent> UploadFilebyStudents { get; set; } = new List<UploadFilebyStudent>();
}
