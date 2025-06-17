using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetBorrowedBookData
    {
        public long BookID { get; set; }
        public string Title { get; set; }
        public double? Borrowed { get; set; }
        public decimal? Available { get; set; }
        //public string Date { get; set; }
        public List<string> Borrowers { get; set; }
    }
}
