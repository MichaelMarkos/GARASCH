using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class AssignedSubject
{
    [Key]
    public int Id { get; set; }

    public int SubjectId { get; set; }

    public int AcademiclevelId { get; set; }

    public int AcademicYearId { get; set; }

    public int SpecialdeptId { get; set; }

    public int CompetitionId { get; set; }

    public int? ProgrammId { get; set; }

    [ForeignKey("AcademicYearId")]
    [InverseProperty("AssignedSubjects")]
    public virtual AcademicYear AcademicYear { get; set; }

    [ForeignKey("AcademiclevelId")]
    [InverseProperty("AssignedSubjects")]
    public virtual Academiclevel Academiclevel { get; set; }

    [ForeignKey("CompetitionId")]
    [InverseProperty("AssignedSubjects")]
    public virtual Competition Competition { get; set; }

    [ForeignKey("ProgrammId")]
    [InverseProperty("AssignedSubjects")]
    public virtual Programm Programm { get; set; }

    [ForeignKey("SpecialdeptId")]
    [InverseProperty("AssignedSubjects")]
    public virtual Specialdept Specialdept { get; set; }

    [ForeignKey("SubjectId")]
    [InverseProperty("AssignedSubjects")]
    public virtual Subject Subject { get; set; }
}
