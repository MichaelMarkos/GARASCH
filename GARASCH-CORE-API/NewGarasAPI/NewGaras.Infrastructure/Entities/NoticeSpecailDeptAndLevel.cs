using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("NoticeSpecailDeptAndLevel")]
public partial class NoticeSpecailDeptAndLevel
{
    [Key]
    public int Id { get; set; }

    public int? SpecialdeptId { get; set; }

    public int? AcademicYearId { get; set; }

    public int? NoticesId { get; set; }

    [ForeignKey("AcademicYearId")]
    [InverseProperty("NoticeSpecailDeptAndLevels")]
    public virtual Academiclevel AcademicYear { get; set; }

    [ForeignKey("NoticesId")]
    [InverseProperty("NoticeSpecailDeptAndLevels")]
    public virtual Notice Notices { get; set; }

    [ForeignKey("SpecialdeptId")]
    [InverseProperty("NoticeSpecailDeptAndLevels")]
    public virtual Specialdept Specialdept { get; set; }
}
