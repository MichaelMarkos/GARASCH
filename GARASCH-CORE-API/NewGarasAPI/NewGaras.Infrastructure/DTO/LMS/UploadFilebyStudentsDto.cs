
namespace NewGaras.Infrastructure.DTO.LMS
{
    public class UploadFilebyStudentsDto
    {
        public int? Id { get; set; }
        public long UserId { get; set; }
        public int CompetitionDayId { get; set; }
        public IFormFile Uploadfile { get; set; }
        public string? uploadfile { get; set; }

        public string? comment { get; set; }
        public bool? Active { get; set; }
        public DateTime? DateTime { get; set; }
        public bool Corrected { get; set; }

    }
}
