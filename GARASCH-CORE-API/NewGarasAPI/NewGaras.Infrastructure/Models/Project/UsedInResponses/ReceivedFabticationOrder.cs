using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.UsedInResponses
{
    public class ReceivedFabticationOrder
    {
        public string FabricationOrderId { get; set; }
        public string ProjectId { get; set; }
        public long ProjectManagerID { get; set; }
        public long FabricationOrderNumber { get; set; }
        public string ClientName { get; set; }
        public string ProjectName { get; set; }
        public string Revesion { get; set; }
        public DateTime ReceivingDate { get; set; }
        public DateTime EndDate { get; set; }
        public long RemainingDays { get; set; }
        public string MaterialAvailability { get; set; }
        public string FabricationProgress { get; set; }
        public string QualityInspection { get; set; }

        //public bool RequireCivilFeedBack { get; set; }
        //public string CivilFeedBackResult { get; set; }
        public bool CivilRequestStatus { get; set; }
        public string CivilRequestDate { get; set; }
        public bool RequireFinFeedBack { get; set; }
        public string FinFeedBackResult { get; set; }
        public bool RequireApprovalFeedBack { get; set; }
        public string ApprovalFeedBackResult { get; set; }

        public decimal TotalWorkingHours { get; set; }
        public bool PassQC { get; set; }
        public bool IsPMuser { get; set; }
        public byte[] SerailQR { get; set; }
        public long? StationId { get; set; }
        public string StationName { get; set; }
    }


}
