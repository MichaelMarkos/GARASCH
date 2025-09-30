using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Programm")]
public partial class Programm
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public double ApprovedHours { get; set; }

    [InverseProperty("Program")]
    public virtual ICollection<Academiclevel> Academiclevels { get; set; } = new List<Academiclevel>();

    [InverseProperty("Programm")]
    public virtual ICollection<AssignedSubject> AssignedSubjects { get; set; } = new List<AssignedSubject>();

    [InverseProperty("Programm")]
    public virtual ICollection<ResultControlForProgram> ResultControlForPrograms { get; set; } = new List<ResultControlForProgram>();

    [InverseProperty("Programm")]
    public virtual ICollection<ResultControlForStudent> ResultControlForStudents { get; set; } = new List<ResultControlForStudent>();

    [InverseProperty("Programm")]
    public virtual ICollection<ResultControl> ResultControls { get; set; } = new List<ResultControl>();
}
