﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("PurchasePOInvoiceTaxIncluded")]
public partial class PurchasePoinvoiceTaxIncluded
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("POInvoiceID")]
    public long PoinvoiceId { get; set; }

    [Column("POTaxIncludedTypeID")]
    public long PotaxIncludedTypeId { get; set; }

    public bool Active { get; set; }

    public long CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    public long? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModificationDate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Percentage { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? Amount { get; set; }

    [Column("CurrencyID")]
    public int? CurrencyId { get; set; }

    [Column("RateToEGP", TypeName = "decimal(18, 5)")]
    public decimal? RateToEgp { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("PurchasePoinvoiceTaxIncludedCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("PurchasePoinvoiceTaxIncludeds")]
    public virtual Currency Currency { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("PurchasePoinvoiceTaxIncludedModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }

    [ForeignKey("PoinvoiceId")]
    [InverseProperty("PurchasePoinvoiceTaxIncludeds")]
    public virtual PurchasePoinvoice Poinvoice { get; set; }

    [ForeignKey("PotaxIncludedTypeId")]
    [InverseProperty("PurchasePoinvoiceTaxIncludeds")]
    public virtual PurchasePoinvoiceTaxIncludedType PotaxIncludedType { get; set; }
}
