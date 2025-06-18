using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Priest")]
public partial class Priest
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [StringLength(250)]
    public string PriestName { get; set; }

    [InverseProperty("Priest")]
    public virtual ICollection<HrUserPriest> HrUserPriests { get; set; } = new List<HrUserPriest>();
}
