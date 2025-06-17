using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Models.AccountAndFinance;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryItemInfoForInsert : InventoryItemInfo
    {
        public int? CalculationTypeID { get; set; }
        public decimal? CustomAmount { get; set; }
        public decimal? CostAmount1 { get; set; }
        public decimal? CostAmount2 { get; set; }
        public decimal? CostAmount3 { get; set; }

        public string CostName1 { get; set; }
        public string CostName2 { get; set; }
        public string CostName3 { get; set; }
        public int? PurchasingUOMID { get; set; }
        public int? RequstionUOMID { get; set; }
        public int? PriorityID { get; set; }
        public string PriorityName { get; set; }
        public long? ParentItemID { get; set; }

        public IFormFile Image { get; set; }

        public string ImageUrl { get; set; }
        // Attachment

        //public string ImageFileName { get; set; }
        //public string ImageFileExtension { get; set; }
        //public string ImageFileContent { get; set; }
        public List<AttachmentFile> AttachmentsList { get; set; }
    }
}
