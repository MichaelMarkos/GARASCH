using NewGaras.Infrastructure.Models.Medical.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical
{
    public class GetDoctorScheduleGroupByDocName
    {
        public long DoctorID { get; set; }
        public string DoctorName { get; set; }
        public long specialityID { get; set; }
        public string specialityName { get; set; }
        public List<DoctorScheduleDTOForGrouping> DoctorScheduleList { get; set; }
    }
}
