using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO
{
    public class MembersOfFamilyDTO
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public int RelationID { get; set; }
        public string RelationName { get; set; }
        public bool Active { get; set; }
        public bool? IsHeadOfTheFamily { get; set; }
    }
}
