using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalaryAllownce
{
    public class EditSalaryAllownce
    {
        public int ID { get; set; }
        public long SalaryId { get; set; }
        public int AllowanceTypeID { get; set; }
        public decimal Percentage { get; set; }

        public decimal Amount { get; set; }

        public bool Active { get; set; }

      
    }
}
