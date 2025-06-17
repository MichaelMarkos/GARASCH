using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.UsedInResponses
{
    public class ProjectsSummary
    {
        [DataMember]
        public string ProjectsType { get; set; } //(ProjectOrders, RentOrders, MaintenanceOrders, WarrantyOrders, InternalOrders)
        [DataMember]
        public string ProjectsStatus { get; set; } //(Open, Closed, Deactivated)
        [DataMember]
        public int CountOfProjects { get; set; }
        [DataMember]
        public decimal TotalCost { get; set; }
        [DataMember]
        public decimal TotalCollectedCost { get; set; }
        [DataMember]
        public string TotalCollectedPercentage { get; set; }
    }
}
