using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EmailTool.Filters
{
    public class GetAllEmailsForUserDBFilters
    {
        [FromHeader]
        public string UserName { get; set; }
        [FromHeader]
        public string KeyWord { get; set; }
        [FromHeader]
        public DateTime? dateFrom { get; set; }
        [FromHeader]
        public DateTime? dateTo { get; set; }
        [FromHeader]
        public int? emailTypeID { get; set; }
        [FromHeader]
        public long? EmailCategoryTypeID { get; set; }

        //[FromHeader]
        //public long? TypeID { get; set; }
        [FromHeader]
        public int currentPage { get; set; } = 1;
        [FromHeader]
        public int itemsPerPage { get; set; } = 10;
    }
}
