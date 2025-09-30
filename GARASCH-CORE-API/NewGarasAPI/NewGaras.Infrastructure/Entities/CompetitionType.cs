using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class CompetitionType
{
    [Key]
    public int Id { get; set; }

    public int CompetitionId { get; set; }

    public int TypeId { get; set; }

    public int Qty { get; set; }

    public double TotalScore { get; set; }

    public bool? AllowAudience { get; set; }

    [Column("percentage")]
    public double? Percentage { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("CompetitionTypes")]
    public virtual Competition Competition { get; set; }

    [ForeignKey("TypeId")]
    [InverseProperty("CompetitionTypes")]
    public virtual Type Type { get; set; }
}
