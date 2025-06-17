using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model
{
    [Serializable]
    public class InvoiceLineModel
    {
        double? Quantity;
        decimal? ItemPrice;
        decimal? DiscountPercentage;
        double? salesTotal;
        double? DiscountValue;
        double? netTotal;
        string ItemTypeEINVOICE;
        string internalCode;
        string ItemCode;
        string UOM_CODE;
        long SalesOfferProductID;
        long? itemID;
        public InvoiceLineModel(double? quantity, decimal? itemPrice, decimal? discountPercentage, double? salesTotal, double? discountValue, double? netTotal, string itemTypeEINVOICE, string internalCode, string itemCode, string uOM_CODE, long salesOfferProductID, long? itemID)
        {
            Quantity1 = quantity;
            ItemPrice1 = itemPrice;
            DiscountPercentage1 = discountPercentage;
            SalesTotal = salesTotal;
            DiscountValue1 = discountValue;
            NetTotal = netTotal;
            ItemTypeEINVOICE1 = itemTypeEINVOICE;
            InternalCode = internalCode;
            ItemCode1 = itemCode;
            UOM_CODE1 = uOM_CODE;
            SalesOfferProductID1 = salesOfferProductID;
            this.itemID = itemID;
        }

        public double? Quantity1 { get => Quantity; set => Quantity = value; }
        public decimal? ItemPrice1 { get => ItemPrice; set => ItemPrice = value; }
        public decimal? DiscountPercentage1 { get => DiscountPercentage; set => DiscountPercentage = value; }
        public double? SalesTotal { get => salesTotal; set => salesTotal = value; }
        public double? DiscountValue1 { get => DiscountValue; set => DiscountValue = value; }
        public double? NetTotal { get => netTotal; set => netTotal = value; }
        public string ItemTypeEINVOICE1 { get => ItemTypeEINVOICE; set => ItemTypeEINVOICE = value; }
        public string InternalCode { get => internalCode; set => internalCode = value; }
        public string ItemCode1 { get => ItemCode; set => ItemCode = value; }
        public string UOM_CODE1 { get => UOM_CODE; set => UOM_CODE = value; }
        public long SalesOfferProductID1 { get => SalesOfferProductID; set => SalesOfferProductID = value; }
        public long? ItemID { get => itemID; set => itemID = value; }
    }
}
