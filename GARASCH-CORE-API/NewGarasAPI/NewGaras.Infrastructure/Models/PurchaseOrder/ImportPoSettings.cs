using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    public class ImportPoSettings
    {
        long pOId;
        decimal? uSDRate;
        decimal? gBPRate;
        decimal? eURORate;
        decimal? customsdollarRate;

        [DataMember]
        public long POId
        {
            get { return pOId; }
            set { pOId = value; }
        }
        [DataMember]
        public decimal? USDRate
        {
            get { return uSDRate; }
            set { uSDRate = value; }
        }
        [DataMember] public decimal? GBPRate { get { return gBPRate; } set { gBPRate = value; } }

        [DataMember] public decimal? EURORate { get { return eURORate; } set { eURORate = value; } }
        [DataMember] public decimal? CustomsdollarRate { get { return customsdollarRate; } set { customsdollarRate = value; } }

    }

}
