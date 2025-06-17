using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer.InternalTicket
{
    public class SalesOfferProductForInternalTicket
    {
        public long? Id { get; set; }
        public decimal? ItemPrice { get; set; }
        public int? InventoryItemCategoryId { get; set; }
        public string InventoryItemCategoryName { get; set; }
        public long? InventoryItemId { get; set; }
        public string InventoryItemName { get; set; }
        public string ModifiedBy { get; set; }
        public string CreatedBy { get; set; }
        public string ItemPricingComment { get; set; }

    }
}
