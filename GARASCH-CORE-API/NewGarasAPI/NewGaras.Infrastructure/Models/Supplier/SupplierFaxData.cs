﻿using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Supplier
{
    public class SupplierFaxData
    {
        public long SupplierId { get; set; }
        public List<AddSupplierFax> SupplierFaxes { get; set; }
    }
}
