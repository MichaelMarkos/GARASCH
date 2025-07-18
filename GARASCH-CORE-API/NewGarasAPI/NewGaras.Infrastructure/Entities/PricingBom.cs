﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("PricingBOM")]
public partial class PricingBom
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Name { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalPrice { get; set; }

    [Column("CurrencyID")]
    public int CurrencyId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal MaterialTotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal FabricationTotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal InstalltionTotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal ProfitTotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TaxTotal { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal OtherTotal { get; set; }

    public long CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    public long? ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    public bool Active { get; set; }

    [Column("PricingProductID")]
    public long PricingProductId { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("PricingBomCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("PricingBoms")]
    public virtual Currency Currency { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("PricingBomModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }

    [ForeignKey("PricingProductId")]
    [InverseProperty("PricingBoms")]
    public virtual PricingProduct PricingProduct { get; set; }
}
