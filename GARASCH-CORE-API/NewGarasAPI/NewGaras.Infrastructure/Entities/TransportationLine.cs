using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("TransportationLine")]
public partial class TransportationLine
{
    [Key]
    public int Id { get; set; }

    public int TransportationVehicleId { get; set; }

    [Required]
    [Column("lineName")]
    [StringLength(250)]
    public string LineName { get; set; }

    [Column("lineCost", TypeName = "decimal(10, 2)")]
    public decimal LineCost { get; set; }

    [Column("oneWay")]
    public bool OneWay { get; set; }

    [Column("isApproved")]
    public bool IsApproved { get; set; }

    [Column("approvedBy")]
    [StringLength(450)]
    public string ApprovedBy { get; set; }

    [Column("active")]
    public bool Active { get; set; }

    [Column("creationDate", TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    [Column("creationBy")]
    public long CreationBy { get; set; }

    [Column("modifiedBy")]
    public long ModifiedBy { get; set; }

    [Column("modifiedDate", TypeName = "datetime")]
    public DateTime ModifiedDate { get; set; }

    [ForeignKey("CreationBy")]
    [InverseProperty("TransportationLineCreationByNavigations")]
    public virtual User CreationByNavigation { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("TransportationLineModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }

    [InverseProperty("TransportationLine")]
    public virtual ICollection<TransportationLineIncreaseRequestLine> TransportationLineIncreaseRequestLines { get; set; } = new List<TransportationLineIncreaseRequestLine>();

    [ForeignKey("TransportationVehicleId")]
    [InverseProperty("TransportationLines")]
    public virtual TransportationVehicle TransportationVehicle { get; set; }

    [InverseProperty("TransportationLine")]
    public virtual ICollection<TransportationVehicleRoute> TransportationVehicleRoutes { get; set; } = new List<TransportationVehicleRoute>();
}
