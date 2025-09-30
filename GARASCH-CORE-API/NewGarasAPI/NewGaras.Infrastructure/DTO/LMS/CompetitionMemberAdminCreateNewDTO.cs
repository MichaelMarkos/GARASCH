
namespace NewGaras.Infrastructure.DTO.LMS
{
   
      
        public class CompetitionMemberAdminCreateNewDTO
        {
            public int Id { get; set; }
            public int CompetitionId { get; set; }
            public List<long> ApplicationUserIds { get; set; }
            //public string? RolrName { get; set; }
        }
   
}
