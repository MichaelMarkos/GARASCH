using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientInfoVM
    {
        public long ClientID { get; set; }
        public string CommercialRecord { get; set; }
        public string TaxCard { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string FaxNumber { get; set; }
        public string Country { get; set; }
        public string Governorate { get; set; }
    }
}
