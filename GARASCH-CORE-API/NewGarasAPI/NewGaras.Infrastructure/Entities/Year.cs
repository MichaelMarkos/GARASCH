using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class Year
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [InverseProperty("Year")]
    public virtual ICollection<AcademicYear> AcademicYears { get; set; } = new List<AcademicYear>();

    [InverseProperty("Year")]
    public virtual ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
}
