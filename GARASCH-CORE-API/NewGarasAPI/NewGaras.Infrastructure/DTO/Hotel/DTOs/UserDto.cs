
using System.ComponentModel.DataAnnotations.Schema;

namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class UserDto
    {
       // public long Id { get; set; }

        public string Password { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Mobile { get; set; } = null!;

        public byte[]? Photo { get; set; }

        public bool? Active { get; set; }

        public DateTime? CreationDate { get; set; }

        public long? ModifiedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? Modified { get; set; }

        public string LastName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public int? Age { get; set; }

        public string? Gender { get; set; }

        public long? CreatedBy { get; set; }

        public int? BranchId { get; set; }

        public int? DepartmentId { get; set; }

        public int? JobTitleId { get; set; }

        public int? OldId { get; set; }
        public IFormFile? Image { get; set; }
        public string? PhotoURL { get; set; }

    }
}
