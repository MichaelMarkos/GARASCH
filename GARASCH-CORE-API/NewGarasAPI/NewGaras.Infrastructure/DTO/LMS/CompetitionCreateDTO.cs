using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionCreateDTO
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? Objective { get; set; }

        public IFormFile? Image { get; set; }
        public IFormFile? CertificateTempImg { get; set; }

        public bool? ShowAnswers { get; set; }
        public bool? ShowRanks { get; set; }
        public bool? ShowScores { get; set; }
        public bool? ShowCertificate { get; set; }

        [Required]
        public int? Days { get; set; }

        [Required]
        public int? StudyingHours { get; set; }

        public string? Code { get; set; }
        public decimal? SolvedPercent { get; set; }

        public string? Status { get; set; }                                   //new
        public int? Capacity { get; set; }                                    //new

        public bool MoreSubjectAtTimeFlag { get; set; } = false;                                //new

    }
}
