namespace NewGarasAPI.Models.ProjectFabrication.UsedInResponses
{
    public class MiniFabticationOrderCard
    {
        [DataMember]
        public string FabricationOrderId { get; set; }

        [DataMember]
        public string ProjectId { get; set; }

        [DataMember]
        public long? ProjectManagerID { get; set; }

        [DataMember]
        public long FabricationOrderNumber { get; set; }

        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public string ProjectName { get; set; }

        [DataMember]
        public string Revesion { get; set; }

        [DataMember]
        public string ReceivingDate { get; set; }

        [DataMember]
        public string? EndDate { get; set; }

        [DataMember]
        public long RemainingDays { get; set; }

        [DataMember]
        public string FabricationProgress { get; set; }

        [DataMember]
        public string QualityInspection { get; set; }


        [DataMember]
        public bool RequireFinFeedBack { get; set; }

        [DataMember]
        public string FinFeedBackResult { get; set; }

        [DataMember]
        public bool? RequireApprovalFeedBack { get; set; }

        [DataMember]
        public string ApprovalFeedBackResult { get; set; }


        [DataMember]
        public string ProjectSerial { get; set; }

        [DataMember]
        public decimal? TotalWorkingHours { get; set; }

        [DataMember]
        public string PassQC { get; set; }

        [DataMember]
        public string FinFeedBackResultApproved { get; set; }

        [DataMember]
        public string SalesPersonFeedBackResultApproved { get; set; }

        [DataMember]
        public string SalesPersonFeedBackResult { get; set; }
    }


    public class FabticationOrderCard : MiniFabticationOrderCard
    {

        [DataMember]
        public string MaterialAvailability { get; set; }

        //[DataMember] public bool RequireCivilFeedBack { get; set; }
        //[DataMember] public string CivilFeedBackResult { get; set; }
        [DataMember]
        public bool CivilRequestStatus { get; set; }
        [DataMember]
        public string CivilRequestDate { get; set; }
        
        
        [DataMember]
        public bool IsPMuser { get; set; }
       
        [DataMember]
        public long? StationId { get; set; }
        [DataMember]
        public string StationName { get; set; }
    }
}
