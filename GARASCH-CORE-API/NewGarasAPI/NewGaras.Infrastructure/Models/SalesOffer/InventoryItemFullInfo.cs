using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.AccountAndFinance;
using NewGarasAPI.Models.User;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class InventoryItemFullInfo : InventoryItemInfo
    {
        public List<SelectDDL> BOMsList { get; set; }
        public List<GetInventoryItemPrice> PricesList { get; set; }
        public List<StoreStockModel> StoresStockList { get; set; }
        public decimal TotalBalance { get; set; }
    }
}