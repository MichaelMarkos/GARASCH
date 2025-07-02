using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class Room
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public int RoomTypeId { get; set; }

    public int BuildingId { get; set; }

    public int RoomViewId { get; set; }

    [Required]
    public string Description { get; set; }

    [Column("capacity")]
    public int? Capacity { get; set; }

    [ForeignKey("BuildingId")]
    [InverseProperty("Rooms")]
    public virtual Building Building { get; set; }

    [InverseProperty("Room")]
    public virtual ICollection<Childern> Childerns { get; set; } = new List<Childern>();

    [InverseProperty("Room")]
    public virtual ICollection<Rate> Rates { get; set; } = new List<Rate>();

    [ForeignKey("RoomTypeId")]
    [InverseProperty("Rooms")]
    public virtual RoomType RoomType { get; set; }
}
