using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("HrUserAttachment")]
public partial class HrUserAttachment
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("HrUserID")]
    public long HrUserId { get; set; }

    [Column("AttachmentTypeID")]
    public long AttachmentTypeId { get; set; }

    [StringLength(250)]
    public string AttachmentNumber { get; set; }

    public long CreatedBy { get; set; }

    public long ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [Required]
    public string AttachmentPath { get; set; }

    [ForeignKey("AttachmentTypeId")]
    [InverseProperty("HrUserAttachments")]
    public virtual AttachmentType AttachmentType { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("HrUserAttachmentCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("Id")]
    [InverseProperty("HrUserAttachment")]
    public virtual HrUser IdNavigation { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("HrUserAttachmentModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }
}
