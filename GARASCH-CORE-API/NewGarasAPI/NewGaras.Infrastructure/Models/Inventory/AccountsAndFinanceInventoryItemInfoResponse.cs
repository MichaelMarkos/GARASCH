﻿using NewGaras.Infrastructure.Models.AccountAndFinance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class AccountsAndFinanceInventoryItemInfoResponse
    {
        public InventoryItemInfo InventoryItemInfo {  get; set; }
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
