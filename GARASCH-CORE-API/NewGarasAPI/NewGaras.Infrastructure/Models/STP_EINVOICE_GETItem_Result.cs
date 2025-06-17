using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class STP_EINVOICE_GETItem_Result
    {
        public string ItemTypeEINVOICE { get; set; }
        public string internalCode { get; set; }
        public string ItemCode { get; set; }
        public string codeName { get; set; }
        public string codeDescription { get; set; }
        public string UOM_CODE { get; set; }
    }
}
