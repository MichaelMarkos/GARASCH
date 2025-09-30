using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("SubjectRelationship")]
public partial class SubjectRelationship
{
    [Key]
    public int Id { get; set; }

    public int MainSubjectId { get; set; }

    public int SubSubjectId { get; set; }

    [Required]
    public string Status { get; set; }

    [ForeignKey("MainSubjectId")]
    [InverseProperty("SubjectRelationshipMainSubjects")]
    public virtual Subject MainSubject { get; set; }

    [ForeignKey("SubSubjectId")]
    [InverseProperty("SubjectRelationshipSubSubjects")]
    public virtual Subject SubSubject { get; set; }
}
