using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Priest")]
public partial class Priest
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [StringLength(250)]
    public string PriestName { get; set; }

    public long CreatedBy { get; set; }

    public long ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("PriestCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [InverseProperty("Priest")]
    public virtual ICollection<HrUserPriest> HrUserPriests { get; set; } = new List<HrUserPriest>();

    [ForeignKey("ModifiedBy")]
    [InverseProperty("PriestModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }
}
