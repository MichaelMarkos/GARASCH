﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class SalesPersonsProductResponse
    {
        public List<SalesPersonProducts> SalesPersonsProductsList { get; set; }

        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
