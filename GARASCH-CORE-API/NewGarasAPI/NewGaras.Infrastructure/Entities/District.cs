using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("District")]
public partial class District
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [StringLength(250)]
    public string DistrictName { get; set; }

    [InverseProperty("District")]
    public virtual ICollection<HrUserAddress> HrUserAddresses { get; set; } = new List<HrUserAddress>();
}
