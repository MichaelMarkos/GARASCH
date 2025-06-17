using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical
{
    public class GetPatientsListModel
    {
        public List<GetPatientModel> Patients { get; set; }
        public PaginationHeader paginationHeader { get; set; }
    }
}
