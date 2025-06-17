namespace NewGarasAPI.Models.ProjectsDetails.UsedInResponses
{
    public class ProjectInstallationCards
    {
        [DataMember]
        public long? ProjectId { get; set; }

        [DataMember]
        public string? ProjectSerial { get; set; }

        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public string InstallationSerial { get; set; }

        [DataMember]
        public decimal? TotalWorkingHours { get; set; }

        [DataMember]
        public int RemaningDays { get; set; }

        [DataMember]
        public int? InstallationOrderNumber { get; set; }

        [DataMember]
        public int InstallationProgress { get; set; }

        [DataMember]
        public string QualityInspection { get; set; }

        [DataMember]
        public bool RequireFinFeedBack { get; set; }

        [DataMember]
        public string FinFeedBackResult { get; set; }

        [DataMember]
        public bool RequireSalesPersonFeedBack { get; set; }

        [DataMember]
        public string SalesPersonFeedBackResult { get; set; }

        //-----------------------not in MVC to be used in flutter--------------------------

        [DataMember]
        public string FinFeedBackResultApproved { get; set; }

        [DataMember]
        public string SalesPersonFeedBackResultApproved { get; set; }
    }
}
