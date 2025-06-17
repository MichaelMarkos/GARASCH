using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class MatrialRequestInfoDetails
    {
        public string StoreName { get; set; }
        public string UserName { get; set; }
        public string InsuranceName { get; set; }
        public string CreatorName { get; set; }
        public string Status { get; set; }
        public long Id { get; set; }
        public string CreationDate { get; set; }
        public List<MatrialRequestItemModel> MatrialReleaseItemList { get; set; }
        public long UserId { get; set; }
        public int StoreId { get; set; }
        public int? UserInsuranceId { get; set; }
    }
}
