
using System.ComponentModel.DataAnnotations;


namespace NewGaras.Infrastructure.DTO.Hotel.DTOs.Auth
{
    public class AddRoleModel
    {
        [Required]
        public long UserId { get; set; }

        [Required]
        public int RoleId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }

    }
}
