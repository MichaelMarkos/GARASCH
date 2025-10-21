
using System.ComponentModel.DataAnnotations;


namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionDayUserExcelCreateDTO

    {
        [Required]
        public int? CompetitionDayId { get; set; }
        //[Required]
        //public decimal? FromScore { get; set; }
        [Required]
        public IFormFile? ExcelSheet { get; set; }

        [MaxLength(100)]
        public string? CreationBy { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
