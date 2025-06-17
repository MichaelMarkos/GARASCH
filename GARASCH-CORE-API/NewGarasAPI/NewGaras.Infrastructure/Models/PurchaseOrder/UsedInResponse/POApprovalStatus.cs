using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder.UsedInResponse
{
    public class POApprovalStatus
    {
        public string ApprovalName { get; set; }
        public bool IsApproved { get; set; }
        public string ApprovalUser { get; set; }
        public string Comment { get; set; }
    }

}
