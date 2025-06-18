using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("PersonStatus")]
public partial class PersonStatus
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [StringLength(250)]
    public string StatusName { get; set; }

    public string Description { get; set; }

    [InverseProperty("PersonStatus")]
    public virtual ICollection<HrUserStatus> HrUserStatuses { get; set; } = new List<HrUserStatus>();
}
