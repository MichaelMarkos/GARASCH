
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewGaras.Infrastructure.Entities;

public partial class Specialdept
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Column("deptartmentId")]
    public int DeptartmentId { get; set; }

    [InverseProperty("Specialdept")]
    public virtual ICollection<AssignedSubject> AssignedSubjects { get; set; } = new List<AssignedSubject>();

    [ForeignKey("DeptartmentId")]
    [InverseProperty("Specialdepts")]
    public virtual Dept Deptartment { get; set; }

    [InverseProperty("Specialdept")]
    public virtual ICollection<NoticeSpecailDeptAndLevel> NoticeSpecailDeptAndLevels { get; set; } = new List<NoticeSpecailDeptAndLevel>();

    [InverseProperty("Specialdept")]
    public virtual ICollection<ResultControl> ResultControls { get; set; } = new List<ResultControl>();

    [InverseProperty("Specialdept")]
    public virtual ICollection<UserDepartment> UserDepartments { get; set; } = new List<UserDepartment>();
}
