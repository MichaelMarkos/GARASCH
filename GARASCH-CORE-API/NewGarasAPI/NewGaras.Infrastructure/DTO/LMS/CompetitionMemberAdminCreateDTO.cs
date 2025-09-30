

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionMemberAdminCreateDTO
    {
        public int Id { get; set; }
        public int CompetitionId { get; set; }
        public long? ApplicationUserId { get; set; }
        //public string? RolrName { get; set; }
    }
}
