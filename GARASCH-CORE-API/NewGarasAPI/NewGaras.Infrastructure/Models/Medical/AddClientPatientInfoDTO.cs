using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Medical
{
    public class AddClientPatientInfoDTO
    {
        public string ClientName { get; set; }
        //public string ClientType { get; set; }
        public long salesPersonID { get; set; }
        public string Email { get; set; }
        public string ClientMobile { get; set; }
        //public int FollowUpPeriod { get; set; }
        public long IdentityNumber { get; set; }
        public long NationalityId { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        //public long ClientId { get; set; }
    }
}
