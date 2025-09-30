using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class Type
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [InverseProperty("Type")]
    public virtual ICollection<CompetitionDay> CompetitionDays { get; set; } = new List<CompetitionDay>();

    [InverseProperty("Type")]
    public virtual ICollection<CompetitionType> CompetitionTypes { get; set; } = new List<CompetitionType>();
}
