using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("City")]
public partial class City
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [Column("GovernorateID")]
    public int? GovernorateId { get; set; }

    [InverseProperty("City")]
    public virtual ICollection<District> Districts { get; set; } = new List<District>();

    [ForeignKey("GovernorateId")]
    [InverseProperty("Cities")]
    public virtual Governorate Governorate { get; set; }

    [InverseProperty("City")]
    public virtual ICollection<HrUserAddress> HrUserAddresses { get; set; } = new List<HrUserAddress>();
}
