

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionDayCreateDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public int? DayNumber { get; set; }
        [Required]
        public int? CompetitionId { get; set; }
        // [RegularExpression(@"^([012]\d | 30 | 31)/(0\d|10|11|12)/\d{4}$)")]         //NEW

        public DateTime? Date { get; set; }
        [Required]
        public DateTime? From { get; set; }
        [Required]
        public DateTime? To { get; set; }
        public string? CompetitionURL { get; set; }
        public string? AnswerURL { get; set; }
        public string? SpreadSheetId { get; set; }
        public string? SheetName { get; set; }

        public string? NameEntryId { get; set; }

        public string? MobileEntryId { get; set; }

        public string? ChurchEntryId { get; set; }

        public string? UserEntryId { get; set; }
        public string? ContentCompetitionDay { get; set; }
        [Precision(18 , 4)]
        public decimal? FromScore { get; set; }

        public int? HallId { get; set; }           //new
        public int TypeId { get; set; }                   //new
        public CompetitionResources? Resources { get; set; }        //new

        public long? lecturerId { get; set; }           //new



        //------------------------------------------------------------------
        public class CompetitionResources
        {
            public int? Id { get; set; }
            public int? CompetitionDayId { get; set; }
            public string? bookId { get; set; }
            public string? fromChapterId { get; set; }
            public string? toChapterId { get; set; }
            public string? bookName { get; set; }
            public string? fromChapterName { get; set; }
            public string? toChapterName { get; set; }
            public string? youtubeLink { get; set; }
            public string? youtubeTitle { get; set; }

            public string? youtubeLink2 { get; set; }
            public string? youtubeTitle2 { get; set; }
            public string? youtubeLink3 { get; set; }
            public string? youtubeTitle3 { get; set; }
            public string? PDFLink1 { get; set; }
            public string? PDTitle1 { get; set; }
            public string? PDFLink2 { get; set; }
            public string? PDTitle2 { get; set; }
            public string? PDFLink3 { get; set; }
            public string? PDTitle3 { get; set; }
            public DateTime? CreationDate { get; set; }
            public string? CreationBy { get; set; }
        }


    }
}
