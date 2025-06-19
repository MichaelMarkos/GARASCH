using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Church")]
public partial class Church
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [StringLength(250)]
    public string ChurchName { get; set; }

    public int? EparchyId { get; set; }

    [ForeignKey("EparchyId")]
    [InverseProperty("Churches")]
    public virtual Eparchy Eparchy { get; set; }

    [InverseProperty("BelongToChurch")]
    public virtual ICollection<HrUser> HrUserBelongToChurches { get; set; } = new List<HrUser>();

    [InverseProperty("ChurchOfPresence")]
    public virtual ICollection<HrUser> HrUserChurchOfPresences { get; set; } = new List<HrUser>();

    [InverseProperty("Church")]
    public virtual ICollection<Priest> Priests { get; set; } = new List<Priest>();
}
