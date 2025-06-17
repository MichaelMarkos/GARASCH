using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class BaseResponseWithIDsList
    {
        public bool Result { get; set; }
        public List<long> IDS { get; set; }
        public List<Error> Errors { get; set; }
    }
}
