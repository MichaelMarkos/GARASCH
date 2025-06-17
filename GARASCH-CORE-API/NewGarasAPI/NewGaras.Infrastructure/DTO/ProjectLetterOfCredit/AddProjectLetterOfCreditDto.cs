using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ProjectLetterOfCredit
{
    public class AddProjectLetterOfCreditDto
    {
        public long ProjectID { get; set; }
        public int LetterOfCreditTypeID { get; set; }
        public int ReturnedAfter { get; set; }
        public string bankName { get; set; }
        public string StartDate { get; set; }
        public decimal Amount { get; set; }
        public int CurrencyID { get; set; }
        public string EndDate { get; set; }
        //public string status { get; set; }


    }
}
