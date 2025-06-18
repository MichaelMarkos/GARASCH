using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("HrUserStatus")]
public partial class HrUserStatus
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("HrUserID")]
    public long HrUserId { get; set; }

    [Column("PersonStatusID")]
    public int PersonStatusId { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("HrUserStatuses")]
    public virtual HrUser HrUser { get; set; }

    [ForeignKey("PersonStatusId")]
    [InverseProperty("HrUserStatuses")]
    public virtual PersonStatus PersonStatus { get; set; }
}
