using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.Responses
{

    public class AddInventoryItemOpeningBalanceRequest
    {
        public List<InventoryItemOpeningBalanceHead> InventoryItemOpeningBalanceHeadList { set; get; }

       
    }
}
