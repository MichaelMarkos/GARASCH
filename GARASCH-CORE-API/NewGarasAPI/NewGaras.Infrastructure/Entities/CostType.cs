﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("CostType")]
public partial class CostType
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Name { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    public long CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("CostTypes")]
    public virtual User CreatedByNavigation { get; set; }

    [InverseProperty("CostType")]
    public virtual ICollection<RequieredCost> RequieredCosts { get; set; } = new List<RequieredCost>();
}
