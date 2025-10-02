
using System.ComponentModel.DataAnnotations;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionDayResourceDTO
    {
        public int Id { get; set; }
        public int CompetitionDayId { get; set; }
        public string bookId { get; set; }
        public string fromChapterId { get; set; }
        public string toChapterId { get; set; }

        [MaxLength(250)]
        public string? bookName { get; set; }
        [MaxLength(250)]
        public string? fromChapterName { get; set; }
        [MaxLength(250)]
        public string? toChapterName { get; set; }


        public string? youtubeLink { get; set; }
        [MaxLength(450)]
        public string? youtubeTitle { get; set; }

        public string? PDFLink1 { get; set; }
        [MaxLength(450)]
        public string? PDTitle1 { get; set; }
    }
}
