using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family
{
    public class GetFamilyCardsDTO
    {
        public long familyId { get; set; }
        public string familyName { get; set; }
        public long? headOfFamilyID  { get; set; }
        public string headOfFamilyName { get; set; }
        public long? churchOfHeadID { get; set; }
        public string churchOfHeadName { get; set; }
        public int familyStatusID { get; set; }
        public string familyStatusName { get; set; }
        public long? servantId { get; set; }
        public int NUmberOFMembersInFamily { get;set; }
    }
}
