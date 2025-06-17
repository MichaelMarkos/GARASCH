using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse
{
    public class InventoryMatrialPurchaseRequestInfo
    {
        public string PurchaseRequestID { get; set; }
        public string FromInventoryStoreName { get; set; }
        public string Status { get; set; }
        public string ApprovalStataus { get; set; }
        public bool IsDirectPR { get; set; }
        public string RequestDate { get; set; }
        public string CreatorName { get; set; }
    }
}
