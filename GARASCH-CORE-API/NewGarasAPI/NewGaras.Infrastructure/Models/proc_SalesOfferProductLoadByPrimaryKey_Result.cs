using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class proc_SalesOfferProductLoadByPrimaryKey_Result
    {
        public long ID { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreationDate { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public bool Active { get; set; }
        public Nullable<long> ProductID { get; set; }
        public long OfferID { get; set; }
        public Nullable<int> ProductGroupID { get; set; }
        public Nullable<double> Quantity { get; set; }
        public Nullable<long> InventoryItemID { get; set; }
        public Nullable<int> InventoryItemCategoryID { get; set; }
        public Nullable<decimal> ItemPrice { get; set; }
        public string ItemPricingComment { get; set; }
        public Nullable<double> ConfirmReceivingQuantity { get; set; }
        public string ConfirmReceivingComment { get; set; }
        public Nullable<long> InvoicePayerClientId { get; set; }
        public Nullable<decimal> DiscountPercentage { get; set; }
        public Nullable<decimal> DiscountValue { get; set; }
        public Nullable<decimal> FinalPrice { get; set; }
        public Nullable<decimal> TaxPercentage { get; set; }
        public Nullable<decimal> TaxValue { get; set; }
        public Nullable<double> ReturnedQty { get; set; }
        public Nullable<double> RemainQty { get; set; }
        public Nullable<decimal> ProfitPercentage { get; set; }
        public Nullable<double> ReleasedQty { get; set; }
    }
}
