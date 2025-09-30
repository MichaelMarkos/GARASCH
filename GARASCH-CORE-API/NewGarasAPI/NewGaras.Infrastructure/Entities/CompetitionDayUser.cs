using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class CompetitionDayUser
{
    [Key]
    public int Id { get; set; }

    public int CompetitionDayId { get; set; }

    [Column("HrUserID")]
    public long HrUserId { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? UserScore { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? FromScore { get; set; }

    public bool? IsFinished { get; set; }

    [StringLength(100)]
    public string CreationBy { get; set; }

    public DateTime CreationDate { get; set; }

    public bool? Attendance { get; set; }

    [Column("comments")]
    [StringLength(450)]
    public string Comments { get; set; }

    [ForeignKey("CompetitionDayId")]
    [InverseProperty("CompetitionDayUsers")]
    public virtual CompetitionDay CompetitionDay { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("CompetitionDayUsers")]
    public virtual HrUser HrUser { get; set; }
}
