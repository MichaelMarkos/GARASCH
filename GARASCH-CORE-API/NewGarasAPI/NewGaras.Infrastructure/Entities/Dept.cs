
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewGaras.Infrastructure.Entities;

public partial class Dept
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [InverseProperty("Deptartment")]
    public virtual ICollection<Specialdept> Specialdepts { get; set; } = new List<Specialdept>();
}
