using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family
{
    public class GetFamiliesListDTO
    {
        public long ID { get; set; }
        public string FamilyName { get; set; }
        public int FamilyStatusID { get; set; }
        public string FamilyStatusName { get; set; }
        public List<MembersOfFamilyDTO> membersList { get; set; }
    }
}
