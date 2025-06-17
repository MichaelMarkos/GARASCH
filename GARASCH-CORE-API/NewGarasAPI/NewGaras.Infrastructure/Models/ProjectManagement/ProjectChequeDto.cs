using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectManagement
{
    public class ProjectChequeDto
    {
        public long? Id { get; set; }

        public string ClientName { get; set; }

        public string SalesPersonName { get; set; }

        public string ProjectName { get; set; }

        public string ProjectNumber { get; set; }

        public long? ProjectId { get; set; }

        public DateTime ChequeDate { get; set; }
        public DateTime? WithdrawDate { get; set; }

        public int? ChequeChashingStatusId { get; set; }

        public bool? IsCrossedCheque { get; set; }

        public decimal ChequeAmount { get; set; }
        public int CurrencyId { get; set; }
        public string Bank { get; set; }

        public string BankBranch { get; set; }

        public string RejectCause { get; set; }

        public string Notes { get; set; }

        public IFormFile Attachment { get; set; }

        public bool DeleteAttachment { get; set; } = false;
        public string ChequeNumber { get; set; }

        public long? WithdrawedBy { get; set; }

        public long? MaintenanceForID { get; set; }

        public long? MaintenanceOrderID { get; set; }
    }
}
