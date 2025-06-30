using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("AssetDepreciation")]
public partial class AssetDepreciation
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("DepreciationTypeID")]
    public long DepreciationTypeId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal CostOfAsset { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? YearOfPurchase { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? ResidualValue { get; set; }

    public int ExpectedLifespanPerMonth { get; set; }

    [Column("ProductionUOMID")]
    public long? ProductionUomid { get; set; }

    [Column("ProductionUOMCount")]
    public int? ProductionUomcount { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal DepreciationRate { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RealCost { get; set; }

    public long CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    public long ModifiedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("AssetDepreciationCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("DepreciationTypeId")]
    [InverseProperty("AssetDepreciations")]
    public virtual DepreciationType DepreciationType { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("AssetDepreciationModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }

    [ForeignKey("ProductionUomid")]
    [InverseProperty("AssetDepreciations")]
    public virtual ProductionUom ProductionUom { get; set; }
}
