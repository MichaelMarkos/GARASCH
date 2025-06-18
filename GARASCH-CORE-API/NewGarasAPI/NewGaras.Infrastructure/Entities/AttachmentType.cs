using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("AttachmentType")]
public partial class AttachmentType
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [StringLength(250)]
    public string Type { get; set; }

    [InverseProperty("AttachmentType")]
    public virtual ICollection<HrUserAttachment> HrUserAttachments { get; set; } = new List<HrUserAttachment>();
}
