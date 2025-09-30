using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("CurrentStudent")]
public partial class CurrentStudent
{
    [Key]
    public int Id { get; set; }

    public long HrUserId { get; set; }

    public int? SpecialDeptId { get; set; }

    public int? AcademiclevelId { get; set; }

    public int? YearId { get; set; }

    public bool? AccreditedhoursSystem { get; set; }

    public bool? CurentStudent { get; set; }

    public bool? Active { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("CurrentStudents")]
    public virtual HrUser HrUser { get; set; }
}
