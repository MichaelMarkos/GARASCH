using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Eparchy")]
public partial class Eparchy
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Name { get; set; }

    [InverseProperty("Eparchy")]
    public virtual ICollection<Church> Churches { get; set; } = new List<Church>();
}
