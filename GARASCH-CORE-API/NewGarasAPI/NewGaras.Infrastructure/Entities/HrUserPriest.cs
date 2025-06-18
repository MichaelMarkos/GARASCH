using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("HrUserPriest")]
public partial class HrUserPriest
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("HrUserID")]
    public long HrUserId { get; set; }

    [Column("PriestID")]
    public long PriestId { get; set; }

    public bool IsCurrent { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DateFrom { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime DateTo { get; set; }

    [StringLength(250)]
    public string Reason { get; set; }

    public long CreatedBy { get; set; }

    public long ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("HrUserPriestCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("HrUserPriests")]
    public virtual HrUser HrUser { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("HrUserPriestModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }

    [ForeignKey("PriestId")]
    [InverseProperty("HrUserPriests")]
    public virtual Priest Priest { get; set; }
}
