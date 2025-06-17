﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    public class ActivePODDLResponse
    {
        public List<ActivePODDL> DDLList { get; set; }
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
