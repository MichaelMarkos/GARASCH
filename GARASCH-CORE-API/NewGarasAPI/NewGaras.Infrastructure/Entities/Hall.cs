using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class Hall
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public int Capacity { get; set; }

    public string Location { get; set; }

    [Column("latitude")]
    public double? Latitude { get; set; }

    [Column("longitude")]
    public double? Longitude { get; set; }

    [InverseProperty("Hall")]
    public virtual ICollection<CompetitionDay> CompetitionDays { get; set; } = new List<CompetitionDay>();
}
