using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class Academiclevel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Level { get; set; }

    public double? MinHours { get; set; }

    public double? MaxHours { get; set; }

    public int? ProgramId { get; set; }

    [InverseProperty("Academiclevel")]
    public virtual ICollection<AssignedSubject> AssignedSubjects { get; set; } = new List<AssignedSubject>();

    [InverseProperty("AcademicYear")]
    public virtual ICollection<NoticeSpecailDeptAndLevel> NoticeSpecailDeptAndLevels { get; set; } = new List<NoticeSpecailDeptAndLevel>();

    [ForeignKey("ProgramId")]
    [InverseProperty("Academiclevels")]
    public virtual Programm Program { get; set; }

    [InverseProperty("Academiclevel")]
    public virtual ICollection<ResultControl> ResultControls { get; set; } = new List<ResultControl>();

    [InverseProperty("Academiclevel")]
    public virtual ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
}
