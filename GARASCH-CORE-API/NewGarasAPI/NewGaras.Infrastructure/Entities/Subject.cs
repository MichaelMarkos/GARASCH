using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class Subject
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Objective { get; set; }

    public string ImagePath { get; set; }

    public bool Active { get; set; }

    public int? Days { get; set; }

    public int? StudyingHours { get; set; }

    public int? Accreditedhours { get; set; }

    public string RequiedofSubject { get; set; }

    public int SubjectScore { get; set; }

    public string Code { get; set; }

    [StringLength(100)]
    public string CreationBy { get; set; }

    public DateTime? CreationDate { get; set; }

    [Column("HrUserID")]
    public long? HrUserId { get; set; }

    public double? ApprovedHours { get; set; }

    [Column("GPAScale")]
    public double? Gpascale { get; set; }

    [InverseProperty("Subject")]
    public virtual ICollection<AssignedSubject> AssignedSubjects { get; set; } = new List<AssignedSubject>();

    [ForeignKey("HrUserId")]
    [InverseProperty("Subjects")]
    public virtual HrUser HrUser { get; set; }

    [InverseProperty("MainSubject")]
    public virtual ICollection<SubjectRelationship> SubjectRelationshipMainSubjects { get; set; } = new List<SubjectRelationship>();

    [InverseProperty("SubSubject")]
    public virtual ICollection<SubjectRelationship> SubjectRelationshipSubSubjects { get; set; } = new List<SubjectRelationship>();
}
