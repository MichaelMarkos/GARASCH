using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs
{
    public class Payment
    {
        public String bankName { get; set; }
        public String bankAddress { get; set; }
        public String bankAccountNo { get; set; }
        public String bankAccountIBAN { get; set; }
        public String swiftCode { get; set; }
        public String terms { get; set; }
    }
}