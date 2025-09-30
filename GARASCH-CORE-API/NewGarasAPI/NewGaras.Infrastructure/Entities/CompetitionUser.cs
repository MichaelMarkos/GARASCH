using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class CompetitionUser
{
    [Key]
    public int Id { get; set; }

    public int CompetitionId { get; set; }

    [Column("HrUserID")]
    public long HrUserId { get; set; }

    public string CreationBy { get; set; }

    [Column("CreationDateOFPending")]
    public DateTime? CreationDateOfpending { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalScore { get; set; }

    public string ReasonForWithdrawalRequest { get; set; }

    public DateTime? DateOfWithdrawal { get; set; }

    public DateTime? DateOfWithdrawalRequest { get; set; }

    public string DelayOrWithdrawalStatus { get; set; }

    public string EnrollmentStatus { get; set; }

    public string FinishStatus { get; set; }

    public string WithdrawalRequestStatus { get; set; }

    public string CreationByDelay { get; set; }

    public string CreationByRejectDelaylRequest { get; set; }

    public string CreationByRejectWithdrawalRequest { get; set; }

    public string CreationByWithdrawal { get; set; }

    public DateTime? DateOfDelay { get; set; }

    public DateTime? DateOfDelaylRequest { get; set; }

    public DateTime? DateOfRejectDelaylRequest { get; set; }

    public DateTime? DateOfRejectWithdrawalRequest { get; set; }

    public string DelayRequestStatus { get; set; }

    public string ReasonForDelayRequesr { get; set; }

    public DateTime? DateOfSuspension { get; set; }

    [Column("CreationDateOFApprovedAndReject")]
    public DateTime? CreationDateOfapprovedAndReject { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("CompetitionUsers")]
    public virtual Competition Competition { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("CompetitionUsers")]
    public virtual HrUser HrUser { get; set; }
}
