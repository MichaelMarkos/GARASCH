

using System.ComponentModel.DataAnnotations;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionUserDTO
    {
        public int Id { get; set; }

        [Required]
        public int CompetitionId { get; set; }
        public long HrUserId { get; set; }
        public string? VerifyCode { get; set; }

    }
}
