﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("ClientPhone")]
public partial class ClientPhone
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("ClientID")]
    public long ClientId { get; set; }

    [Required]
    [StringLength(20)]
    public string Phone { get; set; }

    public long CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    public long? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Modified { get; set; }

    public bool Active { get; set; }

    [ForeignKey("ClientId")]
    [InverseProperty("ClientPhones")]
    public virtual Client Client { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("ClientPhoneCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("ClientPhoneModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }
}
