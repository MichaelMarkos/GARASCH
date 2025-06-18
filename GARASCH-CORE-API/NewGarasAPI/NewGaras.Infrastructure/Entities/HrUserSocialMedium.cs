using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class HrUserSocialMedium
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    public string Link { get; set; }

    [StringLength(250)]
    public string Type { get; set; }

    [Column("HrUserID")]
    public long HrUserId { get; set; }

    public long CreatedBy { get; set; }

    public long ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("HrUserSocialMediumCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("HrUserSocialMedia")]
    public virtual HrUser HrUser { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("HrUserSocialMediumModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }
}
