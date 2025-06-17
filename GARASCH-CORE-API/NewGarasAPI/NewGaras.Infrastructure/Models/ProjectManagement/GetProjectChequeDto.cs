using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectManagement
{
    public class GetProjectChequeDto
    {

        public long Id { get; set; }
        public string ClientName { get; set; }
        public string SalesPersonName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectNumber { get; set; }
        public long ProjectId { get; set; }
        public string ChequeDate { get; set; }
        public int ChequeCashingStatusID { get; set; }
        public string ChequeCashingStatus { get; set; }
        public bool IsCrossedCheque { get; set; }
        public decimal ChequeAmount { get; set; }
        public int CurrencyId { get; set; }
        public string Currency { get; set; }
        public string Bank { get; set; }
        public string BankBranch { get; set; }
        public string RejectCause { get; set; }
        public string Notes { get; set; }
        public string AttachmentPath { get; set; }
        public string WithDrawDate { get; set; }
        public string WithDrawedById { get; set; }
        public string WithDrawedByName { get; set; }
        public string ChequeNumber { get; set; }
        public long ClientId {  get; set; }

        public string CreatedBy { get; set; }
        public string CreationDate { get; set;}

        public long? MaintenanceForID { get; set; }

        public long? MaintenanceOrderID { get; set; }
    }
}
