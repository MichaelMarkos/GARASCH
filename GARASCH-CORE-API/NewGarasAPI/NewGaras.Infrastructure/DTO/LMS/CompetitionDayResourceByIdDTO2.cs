

using System.ComponentModel.DataAnnotations;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionDayResourceByIdDTO2
    {
        public int? NumberOfStudents { get; set; }
        public int? numberOfAttendce { get; set; }
        public int competitionDayid { get; set; }
        public string? hallName { get; set; }
        public CompetitionDayByIdDTO CompetitionDayByIdDTO { get; set; }
        public CompetitionDayRescorcesByIdDTO CompetitionDayRescorcesByIdDTO { get; set; }
    }
    public class CompetitionDayByIdDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }


        public int? CompetitionId { get; set; }
        public DateTime? Date { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string? CompetitionURL { get; set; }
        public string? SpreadSheetId { get; set; }
        public string? SheetName { get; set; }
        public string? AnswerURL { get; set; }

        public string? NameEntryId { get; set; }

        public string? MobileEntryId { get; set; }

        public string? ChurchEntryId { get; set; }

        public string? UserEntryId { get; set; }
        public bool Active { get; set; }

        public string? CreationBy { get; set; }
        public decimal? FromScore { get; set; }

        public DateTime? CreationDate { get; set; }
        public string? ContentCompetitionDay { get; set; }

        public int? HallId { get; set; }           //new
        public string? HallName { get; set; }           //New

        public int TypeId { get; set; }                   //new
        public long? lecturerId { get; set; }           //new

        public string? LectureName { get; set; }                   //New


    }

    public class CompetitionDayRescorcesByIdDTO
    {
        public int Id { get; set; }
        public int CompetitionDayId { get; set; }
        [MaxLength(250)]
        public string? bookId { get; set; }
        [MaxLength(250)]
        public string? fromChapterId { get; set; }
        [MaxLength(250)]
        public string? toChapterId { get; set; }
        [MaxLength(250)]
        public string? bookName { get; set; }
        [MaxLength(250)]
        public string? fromChapterName { get; set; }
        [MaxLength(250)]
        public string? toChapterName { get; set; }
        public string? youtubeLink { get; set; }
        [MaxLength(450)]
        public string? youtubeTitle { get; set; }

        public string? youtubeLink2 { get; set; }
        [MaxLength(450)]
        public string? youtubeTitle2 { get; set; }
        public string? youtubeLink3 { get; set; }
        [MaxLength(450)]
        public string? youtubeTitle3 { get; set; }
        public string? PDFLink1 { get; set; }
        [MaxLength(450)]
        public string? PDTitle1 { get; set; }
        public string? PDFLink2 { get; set; }
        [MaxLength(450)]
        public string? PDTitle2 { get; set; }
        public string? PDFLink3 { get; set; }
        [MaxLength(450)]
        public string? PDTitle3 { get; set; }
        public DateTime? CreationDate { get; set; }
        [MaxLength(250)]

        public string? CreationBy { get; set; }


    }
}
