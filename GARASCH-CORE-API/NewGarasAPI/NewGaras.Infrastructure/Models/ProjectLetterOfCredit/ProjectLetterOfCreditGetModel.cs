using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectLetterOfCredit
{
    public class ProjectLetterOfCreditGetModel
    {
        [FromHeader]
        public long? ProjectID { get; set; }
        [FromHeader]
        public long? ClienId { get; set; }
        [FromHeader]
        public long? SalesPersonID { get; set; }
        [FromHeader]
        public string Status { get; set; }
        [FromHeader]
        public string BankName { get; set; }
        [FromHeader]
        public int? LetterOfCreditTypeID { get; set; }
        [FromHeader]
        public int NumberOfitemsPerPage { get; set; } = 10;
        [FromHeader]
        public int currentPage { get; set; } = 1;

    }
}
