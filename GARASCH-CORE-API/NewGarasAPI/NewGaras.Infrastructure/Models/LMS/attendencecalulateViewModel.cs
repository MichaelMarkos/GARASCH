

namespace NewGaras.Infrastructure.Models.LMS
{
    public class attendencecalulateViewModel
    {
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string dateGenerateQr { get; set; }
        //public DateTime dateAfter { get; set; }
        public long userId { get; set; }
        public int CompetitionDayId { get; set; }
    }
}
