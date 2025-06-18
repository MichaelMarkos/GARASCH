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

    [ForeignKey("HrUserId")]
    [InverseProperty("HrUserSocialMedia")]
    public virtual HrUser HrUser { get; set; }
}
