

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitorUserInfoDTO
    {
        public string UserPhoto { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public bool Active { get; set; }
        public string PhoneNumber { get; set; }
        public decimal Score { get; set; }
        public int CountOfDays { get; set; }

        // RAnk and Total RAnk
        public int RankNo { get; set; }
        public string? RoleName { get; set; }
        public int TotalRank { get; set; }
    }
}
