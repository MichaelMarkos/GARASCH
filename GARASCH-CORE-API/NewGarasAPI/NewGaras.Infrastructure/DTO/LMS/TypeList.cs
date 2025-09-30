

using NewGaras.Infrastructure.Models.LMS;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class TypeList
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        public int Qty { get; set; }
        public double? TotalScore { get; set; }
        public decimal? TotalScoreForStudent { get; set; }
        public bool? AllowAudience { get; set; }
        public double? percentage { get; set; }
        public int? remainingNumber { get; set; }
        public List<FilterTabledDto>? specialtypeList { get; set; }
    }
}
