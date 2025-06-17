using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse
{
    public class PODailyJournalEntryDetails
    {
        public int CountOfJournalEntry { get; set; }
        public decimal SumAmountOfJounralEntry { get; set; }
        public decimal SumPlusAmountSupplierAccounts { get; set; }
        public decimal SumMinusAmountSupplierAccounts { get; set; }
        public List<long> ListOfJounralEntries { get; set; }
    }
}
