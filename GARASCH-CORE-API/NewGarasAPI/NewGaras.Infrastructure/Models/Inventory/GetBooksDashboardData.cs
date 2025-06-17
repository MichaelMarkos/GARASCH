using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetBooksDashboardData
    {
        public long TotalBorrowers { get; set; }
        public long TotalBooks { get; set; }
        public long TotalCopies { get; set; }
        public decimal BooksInLibrary { get; set; }
        public decimal BooksBorrowed { get; set; }
    }
}
