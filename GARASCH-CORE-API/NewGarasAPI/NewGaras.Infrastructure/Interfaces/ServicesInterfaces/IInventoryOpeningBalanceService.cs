using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.InventoryItem;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.Responses;
using NewGaras.Infrastructure.Models.TaskMangerProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IInventoryOpeningBalanceService
    {
        public BaseResponseWithId<long> AddInventoryItemOpeningBalance(AddInventoryItemOpeningBalanceRequest Request, long UserID);

        public BaseResponseWithId<long> AddInventoryItemOpeningBalanceForLIBMARK(AddInventoryItemOpeningBalanceRequest Request, long UserID);

        
        public BaseResponseWithData<string> GetInventoryItemOpeningBalancePOSTemplete(string CompName);

        public BaseResponseWithMessage<string> UploadInventoryItemOpeningBalancePOSExcel(AddAttachment dto, long UserID);
        public BaseResponseWithId<long> AddInventoryItemOpeningBalancePOS(AddInventoryItemOpeningBalanceRequest Request, long UserID);
        public BaseResponseWithId<long> UpdateInventoryItemCost(long inventoryStoreItemID, decimal Cost);
    }
}
