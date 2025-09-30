using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("UserDepartment")]
public partial class UserDepartment
{
    [Key]
    public int Id { get; set; }

    public int AcademiclevelId { get; set; }

    public int YearId { get; set; }

    public int SpecialdeptId { get; set; }

    public long HrUserId { get; set; }

    [ForeignKey("AcademiclevelId")]
    [InverseProperty("UserDepartments")]
    public virtual Academiclevel Academiclevel { get; set; }

    [ForeignKey("HrUserId")]
    [InverseProperty("UserDepartments")]
    public virtual HrUser HrUser { get; set; }

    [ForeignKey("SpecialdeptId")]
    [InverseProperty("UserDepartments")]
    public virtual Specialdept Specialdept { get; set; }

    [ForeignKey("YearId")]
    [InverseProperty("UserDepartments")]
    public virtual Year Year { get; set; }
}
