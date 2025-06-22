using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family
{
    public class AddNewFamilyWithMembersDTO
    {
        public string FamilyName { get; set; }
        public int FamilyStatusID { get; set; }
        public List<long> HruserIDList { get; set; }
        public long headOfFamilyID { get; set; }
    }
}
