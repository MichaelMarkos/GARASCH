using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class PortalLoginTokenResponse
    {
        public bool Result { get; set; }
        public string LoginToken { get; set; }
        public List<Error> Errors { get; set; }
    }
}
