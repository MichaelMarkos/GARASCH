using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse
{
    public class PurchasePOByDate
    {
        public string DateMonth { get; set; }
        public List<PurchasePO> PurchasePOList { get; set; }
    }
}
