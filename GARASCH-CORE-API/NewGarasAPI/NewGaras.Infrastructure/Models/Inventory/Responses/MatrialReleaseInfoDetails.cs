using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class MatrialReleaseInfoDetails : MatrialReleaseInfo
    {
        public int? StoreId { get; set; }
        public string StoreName { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }
        public string InsuranceName { get; set; }
        public string CreatorName { get; set; }
        public string Status { get; set; }
        public string DateOfBirth { get; set; }
        public string StatusComment { get; set; }
    }
}
