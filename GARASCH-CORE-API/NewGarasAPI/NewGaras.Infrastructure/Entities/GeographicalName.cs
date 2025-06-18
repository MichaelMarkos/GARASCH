using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("GeographicalName")]
public partial class GeographicalName
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [Column("GeographicalName")]
    [StringLength(250)]
    public string GeographicalName1 { get; set; }

    [InverseProperty("GeographicalName")]
    public virtual ICollection<HrUserAddress> HrUserAddresses { get; set; } = new List<HrUserAddress>();
}
