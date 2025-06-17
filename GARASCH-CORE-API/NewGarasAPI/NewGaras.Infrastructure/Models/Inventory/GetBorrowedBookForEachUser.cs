using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetBorrowedBookForEachUser
    {
        public string UserName { get; set; }
        public int totalBooksBorrowed { get; set; }
        public string LastBorrowedBookName { get; set; }
        public string LastBorrowingDate { get; set; }
    }
}
