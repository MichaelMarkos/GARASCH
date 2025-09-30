

using NewGaras.Infrastructure.DTO.LMS;

namespace NewGaras.Infrastructure.Models.LMS
{
    public class lectureTodayVM
    {
        public FilterTabledDto? comppetitionDayAtTime { get; set; }
        public bool? comppetitionDayListFlag { get; set; }
        public int NumberOfMission { get; set; }
        public int NumberOfQuies { get; set; }
        public int NumberOfNotices { get; set; }
    }
}
