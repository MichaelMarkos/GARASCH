using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class STP_EINVOICE_GETCustomer_Result
    {
        public long ACCOUNT_NUMBER { get; set; }
        public string party_name { get; set; }
        public string COUNTRY { get; set; }
        public string street { get; set; }
        public string regionCity { get; set; }
        public string governate { get; set; }
        public string buildingNumber { get; set; }
        public string TaxpayerCode { get; set; }
        public string floor { get; set; }
        public string landmark { get; set; }
        public string postalCode { get; set; }
        public string room { get; set; }
        public string additionalInformation { get; set; }
        public string Cus_type { get; set; }
    }
}
