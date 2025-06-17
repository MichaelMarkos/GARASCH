namespace NewGarasAPI.Models.ProjectsDetails.ViewModels
{
    public class ManagementOfMaintenanceOfferVM
    {
        [DataMember]
        public long OfferID { get; set; }

        [DataMember]
        public int ProjectID { get; set; }

        [DataMember]
        public int NumberOfVisits { get; set; }

        [DataMember]
        public string StartDate { get; set; }

        [DataMember]
        public string EndDate { get; set; }

        [DataMember]
        public List<string> PlannedDate { get; set; }

        [DataMember]
        public int DifferenceDate { get; set; }

        [DataMember]
        public string ExtraPlannedDate { get; set; }

        [DataMember]
        public long ManagementOfMaintenanceOrderID { get; set; }
    }
}
