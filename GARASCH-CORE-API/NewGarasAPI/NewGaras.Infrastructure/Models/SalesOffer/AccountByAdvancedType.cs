using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class AccountByAdvancedType
    {
        public string AccountName { get; set; }

        public string AdvancedTypeName { get; set; }

        public decimal AccountAmount { get; set; }

        public int? BranchId { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime EntryDate { get; set; }


    }
}
