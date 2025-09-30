using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class Notice
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string Thetopic { get; set; }

    public string Filepath { get; set; }

    public DateTime? Date { get; set; }

    [Column("receiverType")]
    public int? ReceiverType { get; set; }

    public int? CompetitionId { get; set; }

    public long? CreationBy { get; set; }

    public bool? NewsOrAlertsFlag { get; set; }

    public DateTime CreationDate { get; set; }

    [ForeignKey("CreationBy")]
    [InverseProperty("Notices")]
    public virtual HrUser CreationByNavigation { get; set; }

    [InverseProperty("Notices")]
    public virtual ICollection<NoticeSpecailDeptAndLevel> NoticeSpecailDeptAndLevels { get; set; } = new List<NoticeSpecailDeptAndLevel>();
}
