namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetVisitsMaintenanceReportDetailsData
    {
        public long ID { get; set; }
        public long MaintVisitID { get; set; }
        public string ReportDate { get; set; }
        public string ClientAddress { get; set; }
        public string DefectDescription { get; set; }
        public string WorkDescription { get; set; }
        public decimal? CollectedAmount { get; set; }
        public string CurrencyName { get; set; }
        public string ByUser { get; set; }
        public long? ByUserID { get; set; }
        public bool ClientPRStatus { get; set; }
        public bool InstallationPRStatus { get; set; }
        public bool FabricationPRStatus { get; set; }
        public bool DesignPRStatus { get; set; }
        public bool ProductLifePRStatus { get; set; }
        public bool MaintenanceTeamPRStatus { get; set; }
        public string InternalPartComments { get; set; }
        public bool Finished { get; set; }
        public bool CRMFeedbackStatus { get; set; }
        public string CRMFeedback { get; set; }
        public string CRMCommitment { get; set; }
        public string CRMFeedbackComments { get; set; }
        public string ClientCommentsAndFeedback { get; set; }
        public decimal? ClientSatisfactionRate { get; set; }
        public string ClientSignature { get; set; }
        public string ProblemReportImage { get; set; }
        public decimal? MileageCounter { get; set; }

        public List<GetUsersWorkHoursAndEvaluation> GetUsersWorkHoursAndEvaluationList { get; set; }
    }
}