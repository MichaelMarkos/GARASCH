using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("FamilyStatus")]
public partial class FamilyStatus
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [StringLength(250)]
    public string StatusName { get; set; }

    public string Description { get; set; }

    [InverseProperty("FamilyStatus")]
    public virtual ICollection<Family> Families { get; set; } = new List<Family>();
}
