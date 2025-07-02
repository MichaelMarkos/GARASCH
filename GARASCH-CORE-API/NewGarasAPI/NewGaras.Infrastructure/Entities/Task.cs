using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

[Table("Task")]
public partial class Task
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ExpireDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreationDate { get; set; }

    public long CreatedBy { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? ModifiedDate { get; set; }

    public long? ModifiedBy { get; set; }

    public bool Active { get; set; }

    [Required]
    [StringLength(500)]
    public string TaskUser { get; set; }

    [Column("TaskTypeID")]
    public int TaskTypeId { get; set; }

    [Required]
    [StringLength(100)]
    public string RefrenceNumber { get; set; }

    [StringLength(500)]
    public string TaskCategory { get; set; }

    public string TaskSubject { get; set; }

    [Column("BranchID")]
    public int? BranchId { get; set; }

    [StringLength(250)]
    public string TaskPage { get; set; }

    public bool GroupReply { get; set; }

    [Column("ProjectID")]
    public long? ProjectId { get; set; }

    public long? ProjectSprintId { get; set; }

    public long? ProjectWorkFlowId { get; set; }

    public bool? IsArchived { get; set; }

    [ForeignKey("BranchId")]
    [InverseProperty("Tasks")]
    public virtual Branch Branch { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("TaskCreatedByNavigations")]
    public virtual User CreatedByNavigation { get; set; }

    [ForeignKey("ModifiedBy")]
    [InverseProperty("TaskModifiedByNavigations")]
    public virtual User ModifiedByNavigation { get; set; }

    [ForeignKey("ProjectId")]
    [InverseProperty("Tasks")]
    public virtual Project Project { get; set; }

    [ForeignKey("ProjectSprintId")]
    [InverseProperty("Tasks")]
    public virtual ProjectSprint ProjectSprint { get; set; }

    [ForeignKey("ProjectWorkFlowId")]
    [InverseProperty("Tasks")]
    public virtual ProjectWorkFlow ProjectWorkFlow { get; set; }

    [ForeignKey("TaskTypeId")]
    [InverseProperty("Tasks")]
    public virtual TaskType TaskType { get; set; }

    [InverseProperty("Task")]
    public virtual ICollection<WorkingHourseTracking> WorkingHourseTrackings { get; set; } = new List<WorkingHourseTracking>();
}
