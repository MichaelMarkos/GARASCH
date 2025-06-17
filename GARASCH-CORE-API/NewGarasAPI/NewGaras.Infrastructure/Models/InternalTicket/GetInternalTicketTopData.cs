using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InternalTicket
{
    public class GetInternalTicketTopData
    {
        public List<TopData> TopDoctorsList { get; set; }
        public List<TopData> TopDepartmentsList { get; set; }
        public List<TopData> TopCategoriesList { get; set; }
    }
}
