using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("GPACalculations")]
public partial class Gpacalculation
{
    [Key]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RateFrom { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal RateTo { get; set; }

    [Required]
    public string VerbalGrade { get; set; }

    [Column("GPAValue", TypeName = "decimal(18, 2)")]
    public decimal Gpavalue { get; set; }

    [Required]
    public string Grade { get; set; }

    public bool Active { get; set; }
}
