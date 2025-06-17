using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class AggregateMatrialReleaseItemRequest
    {
        public long ParentMatrialReleaseId { get; set; }
        public string Status { get; set; }
        public List<AggregateMatrialReleaseItemPerUser> AggregateMatrialReleaseItemPerUserList { get; set; }
    }

    public class AggregateMatrialReleaseItemPerUser
    {
        public long UserId { get; set; }
        public string? TransactionDate { get; set; }
        public int? UserInsuranceId { get; set; }
        public List<AggregateMatrialReleaseItem> AggregateMatrialReleaseItemList { get; set; }
    }
    public class AggregateMatrialReleaseItem
    {
        public long MatrialReleaseItemId { get; set; }
        public long InventoryItemId { get; set; }
        public decimal QTY { get; set; }
    }
}
