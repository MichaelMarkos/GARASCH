using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Family")]
public partial class Family
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    public string FamilyName { get; set; }

    [Column("FamilyStatusID")]
    public int FamilyStatusId { get; set; }

    public long? ServantId { get; set; }

    [ForeignKey("FamilyStatusId")]
    [InverseProperty("Families")]
    public virtual FamilyStatus FamilyStatus { get; set; }

    [InverseProperty("Family")]
    public virtual ICollection<HrUserFamily> HrUserFamilies { get; set; } = new List<HrUserFamily>();

    [ForeignKey("ServantId")]
    [InverseProperty("Families")]
    public virtual User Servant { get; set; }
}
