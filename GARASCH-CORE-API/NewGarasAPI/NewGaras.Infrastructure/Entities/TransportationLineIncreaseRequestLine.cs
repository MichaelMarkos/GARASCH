using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class TransportationLineIncreaseRequestLine
{
    [Key]
    public long Id { get; set; }

    public int TransportationLineIncreaseRequestId { get; set; }

    [Column("transportationLineId")]
    public int TransportationLineId { get; set; }

    [ForeignKey("TransportationLineId")]
    [InverseProperty("TransportationLineIncreaseRequestLines")]
    public virtual TransportationLine TransportationLine { get; set; }

    [ForeignKey("TransportationLineIncreaseRequestId")]
    [InverseProperty("TransportationLineIncreaseRequestLines")]
    public virtual TransportationLineIncreaseRequest TransportationLineIncreaseRequest { get; set; }
}
