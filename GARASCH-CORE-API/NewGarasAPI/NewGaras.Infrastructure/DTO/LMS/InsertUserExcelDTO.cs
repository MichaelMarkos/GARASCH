
using System.ComponentModel.DataAnnotations;


namespace NewGaras.Infrastructure.DTO.LMS
{
    public class InsertUserExcelDTO
    {
        [Required]
        public IFormFile ExcelSheet { get; set; }

        [MaxLength(100)]
        public string? CreationBy { get; set; }

        public DateTime CreationDate { get; set; }
        public string? FileUrl { get; set; } = " ";
        public string? fileName { get; set; }
    }
}
