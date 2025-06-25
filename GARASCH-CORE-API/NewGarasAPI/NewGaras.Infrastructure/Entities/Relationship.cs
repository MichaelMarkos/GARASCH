using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Relationship")]
public partial class Relationship
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string RelationshipName { get; set; }

    [InverseProperty("Relationship")]
    public virtual ICollection<HrUserFamily> HrUserFamilies { get; set; } = new List<HrUserFamily>();
}
