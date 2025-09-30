using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("UploadFilebyStudent")]
public partial class UploadFilebyStudent
{
    [Key]
    public int Id { get; set; }

    public long HrUserId { get; set; }

    public int CompetitionDayId { get; set; }

    [Required]
    [Column("uploadfile")]
    public string Uploadfile { get; set; }

    [Column("comment")]
    public string Comment { get; set; }

    public DateTime? DateTime { get; set; }

    public bool? Active { get; set; }

    public bool Corrected { get; set; }

    [ForeignKey("CompetitionDayId")]
    [InverseProperty("UploadFilebyStudents")]
    public virtual CompetitionDay CompetitionDay { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("UploadFilebyStudents")]
    public virtual HrUser HrUser { get; set; }
}
