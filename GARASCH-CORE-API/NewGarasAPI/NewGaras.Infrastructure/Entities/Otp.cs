using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Otp")]
public partial class Otp
{
    [Key]
    public int Id { get; set; }

    public long HrUserId { get; set; }

    public int Otpcode { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("Otps")]
    public virtual HrUser HrUser { get; set; }
}
