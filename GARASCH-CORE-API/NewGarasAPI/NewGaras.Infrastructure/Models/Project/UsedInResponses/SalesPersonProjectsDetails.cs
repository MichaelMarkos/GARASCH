
namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class SalesPersonProjectsDetails
    {
        [DataMember] 
        public long SalesPersonId { get; set; }

        [DataMember] 
        public string SalesPersonName { get; set; }

        [DataMember] 
        public int ClientsCount { get; set; }

        [DataMember] 
        public int DealsCount { get; set; }

        [DataMember] 
        public decimal AchievedTarget { get; set; }

        [DataMember] 
        public decimal TotalCollected { get; set; }

        [DataMember] 
        public decimal TotalRemain { get; set; }

        [DataMember] 
        public string CollectedPercentage { get; set; }

        [DataMember] 
        public string RemainPercentage { get; set; }
    }
}
