using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class AddVisitsMaintenanceReportDetailsData
    {
        public long ID { get; set; }
        public long MaintVisitID { get; set; }
        public string ReportDate { get; set; }
        public string ClientAddress { get; set; }
        public string DefectDescription { get; set; }
        public string WorkDescription { get; set; }
        public IFormFile File { get; set; }
     /*   public string FileName { get; set; }
        public string FileExtension { get; set; }*/

        // Problem Image 
        public IFormFile ProbelmImage { get; set; }
/*        public string ProbelmImageFileName { get; set; }
        public string ProbelmImageFileExtension { get; set; }*/

        public decimal? CollectedAmount { get; set; }
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
        public string NextVisitDate { get; set; }
        public decimal? CurrentMileageCounter { get; set; }

        public List<AddUsersWorkHoursAndEvaluation> GetUsersWorkHoursAndEvaluationList { get; set; }
    }
}
