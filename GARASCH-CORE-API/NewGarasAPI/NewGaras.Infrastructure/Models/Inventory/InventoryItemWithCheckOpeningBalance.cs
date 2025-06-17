using NewGaras.Infrastructure.Entities;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Inventory
{
    public class InventoryItemWithCheckOpeningBalance : InventoryItem
    {
        private bool? haveOpeningBalance;

        private int categoryId;
        private int? headParentCategoryId;

        private int requstionUOMID;
        private int purchasingUOMID;
        private bool active;
        private int calculationType;
        private string commericalName;
        private string categoryName;
        private string headParentCategoryName;
        private string description;

        private string details;
        private string marketName;
        private decimal? maxBlanace;
        private decimal? minBalance;
        private decimal? customeUnitPrice;

        private string imageUrl;


        private string publicNo;
        [DataMember]
        public string PublicNo { get => publicNo; set => publicNo = value; }

        [DataMember]
        public decimal? MinBalance { get => minBalance; set => minBalance = value; }
        [DataMember]
        public decimal? MaxBlanace { get => maxBlanace; set => maxBlanace = value; }
        [DataMember]
        public string MarketName { get => marketName; set => marketName = value; }
        [DataMember]
        public decimal? CustomeUnitPrice { get => customeUnitPrice; set => customeUnitPrice = value; }
        [DataMember]
        public string Details { get => details; set => details = value; }
        [DataMember]
        public string Description { get => description; set => description = value; }
        [DataMember]
        public string CommericalName { get => commericalName; set => commericalName = value; }
        [DataMember]
        public int CalculationType { get => calculationType; set => calculationType = value; }
        [DataMember]
        public bool Active { get => active; set => active = value; }
        [DataMember]
        public int PurchasingUOMID { get => purchasingUOMID; set => purchasingUOMID = value; }
        [DataMember]
        public int RequstionUOMID { get => requstionUOMID; set => requstionUOMID = value; }

        [DataMember]
        public int CategoryId { get => categoryId; set => categoryId = value; }

        [DataMember]
        public int? HeadParentCategoryId { get => headParentCategoryId; set => headParentCategoryId = value; }

        [DataMember]
        public string CategoryName { get => categoryName; set => categoryName = value; }

        [DataMember]
        public string HeadParentCategoryName { get => headParentCategoryName; set => headParentCategoryName = value; }

        [DataMember]
        public bool? HaveOpeningBalance { get => haveOpeningBalance; set => haveOpeningBalance = value; }
        [DataMember]
        public string ImageUrl { get => imageUrl; set => imageUrl = value; }
    }
}
