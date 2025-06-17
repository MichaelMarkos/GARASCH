using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InternalTicket
{
    public class GetInternalTicketsByCategoryResponse
    {
        public List<InternalTicketCategories> InternalTicketDepartmentList { get; set; }
    }

    public class GetInternalTicketsByItemCategoryResponse
    {
        public List<InternalTicketItemCategories> InternalTicketDepartmentList { get; set; }
    }
}
