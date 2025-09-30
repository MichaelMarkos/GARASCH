using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class Competition
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Objective { get; set; }

    public string ImagePath { get; set; }

    [StringLength(100)]
    public string CreationBy { get; set; }

    public DateTime? CreationDate { get; set; }

    public string Code { get; set; }

    public int? Days { get; set; }

    public int? StudyingHours { get; set; }

    public bool Active { get; set; }

    public string RequiedofSubject { get; set; }

    [Column("HrUserID")]
    public long? HrUserId { get; set; }

    public int? Accreditedhours { get; set; }

    public int SubjectScore { get; set; }

    public int? Capacity { get; set; }

    public string Status { get; set; }

    public bool MoreSubjectAtTimeFlag { get; set; }

    public bool? CorrectionDone { get; set; }

    [InverseProperty("Competition")]
    public virtual ICollection<AssignedSubject> AssignedSubjects { get; set; } = new List<AssignedSubject>();

    [InverseProperty("Competition")]
    public virtual ICollection<CompetitionDay> CompetitionDays { get; set; } = new List<CompetitionDay>();

    [InverseProperty("Competition")]
    public virtual ICollection<CompetitionMemberAdmin> CompetitionMemberAdmins { get; set; } = new List<CompetitionMemberAdmin>();

    [InverseProperty("Competition")]
    public virtual ICollection<CompetitionType> CompetitionTypes { get; set; } = new List<CompetitionType>();

    [InverseProperty("Competition")]
    public virtual ICollection<CompetitionUser> CompetitionUsers { get; set; } = new List<CompetitionUser>();

    [ForeignKey("HrUserId")]
    [InverseProperty("Competitions")]
    public virtual HrUser HrUser { get; set; }

    [InverseProperty("Competition")]
    public virtual ICollection<ResultControlForProgram> ResultControlForPrograms { get; set; } = new List<ResultControlForProgram>();

    [InverseProperty("Competition")]
    public virtual ICollection<ResultControlForStudent> ResultControlForStudents { get; set; } = new List<ResultControlForStudent>();

    [InverseProperty("Competition")]
    public virtual ICollection<ResultControl> ResultControls { get; set; } = new List<ResultControl>();
}
