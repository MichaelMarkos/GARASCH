using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class CompetitionMemberAdmin
{
    [Key]
    public int Id { get; set; }

    public int CompetitionId { get; set; }

    [Column("HrUserID")]
    public long? HrUserId { get; set; }

    public DateTime? CreationDate { get; set; }

    public string RoleName { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("CompetitionMemberAdmins")]
    public virtual Competition Competition { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("CompetitionMemberAdmins")]
    public virtual HrUser HrUser { get; set; }
}
