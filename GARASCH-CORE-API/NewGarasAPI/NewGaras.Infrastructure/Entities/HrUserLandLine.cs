using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("HrUserLandLine")]
public partial class HrUserLandLine
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [StringLength(50)]
    public string LandLineNumber { get; set; }

    [Column("HrUserID")]
    public long HrUserId { get; set; }

    public long CreatedBy { get; set; }

    public long ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("HrUserLandLineCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("HrUserLandLines")]
    public virtual HrUser HrUser { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("HrUserLandLineModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }
}
