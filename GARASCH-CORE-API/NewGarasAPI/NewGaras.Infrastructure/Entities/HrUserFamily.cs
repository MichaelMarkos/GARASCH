using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("HrUserFamily")]
public partial class HrUserFamily
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("HrUserID")]
    public long HrUserId { get; set; }

    [Column("FamilyID")]
    public long FamilyId { get; set; }

    public bool Active { get; set; }

    public bool? IsHeadOfTheFamily { get; set; }

    [ForeignKey("FamilyId")]
    [InverseProperty("HrUserFamilies")]
    public virtual Family Family { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("HrUserFamilies")]
    public virtual HrUser HrUser { get; set; }
}
