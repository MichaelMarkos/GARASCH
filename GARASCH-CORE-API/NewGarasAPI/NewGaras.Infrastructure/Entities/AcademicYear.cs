using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class AcademicYear
{
    [Key]
    public int Id { get; set; }

    public DateTime From { get; set; }

    [Required]
    public string Term { get; set; }

    public DateTime To { get; set; }

    public int YearId { get; set; }

    [InverseProperty("AcademicYear")]
    public virtual ICollection<AssignedSubject> AssignedSubjects { get; set; } = new List<AssignedSubject>();

    [InverseProperty("AcademicYear")]
    public virtual ICollection<ResultControlForProgram> ResultControlForPrograms { get; set; } = new List<ResultControlForProgram>();

    [InverseProperty("AcademicYear")]
    public virtual ICollection<ResultControlForStudent> ResultControlForStudents { get; set; } = new List<ResultControlForStudent>();

    [InverseProperty("AcademicYear")]
    public virtual ICollection<ResultControl> ResultControls { get; set; } = new List<ResultControl>();

    [ForeignKey("YearId")]
    [InverseProperty("AcademicYears")]
    public virtual Year Year { get; set; }
}
