using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class MatrialReleaseIndex
    {
        public long Id { get; set; }
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string LocationName { get; set; }
        public string Date { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string CreatorName { get; set; }
        public string Status { get; set; }
        public string StatusComment { get; set; }
        public string ReviewedBy { get; set; }
        public bool IsFinished { get; set; }
        public long? MatrialRequestId { get; set; }
    }
}
