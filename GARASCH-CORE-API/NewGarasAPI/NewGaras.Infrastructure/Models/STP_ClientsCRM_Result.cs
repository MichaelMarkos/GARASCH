using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class STP_ClientsCRM_Result
    {
        public string SupportedBy { get; set; }
        public int OldClientsCount { get; set; }
        public int OldClientsRFQCount { get; set; }
        public int OldDealedClients { get; set; }
        public int OldClientsDeals { get; set; }
        public decimal OldClientsProjectExtraModifications { get; set; }
        public decimal TotalDealsPriceOldClients { get; set; }
        public int NewClientsCount { get; set; }
        public int NewClientsRFQCount { get; set; }
        public int NewClientsCountLastYear { get; set; }
        public int NewDealedClients { get; set; }
        public int NewClientsDeals { get; set; }
        public decimal NewClientsProjectExtraModifications { get; set; }
        public decimal TotalDealsPriceNewClients { get; set; }
        public decimal TotalDealsPriceNewClientsLastYear { get; set; }
    }
}
