using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family
{
    public class EditTheHeadOfFamilyDTO
    {
        public long familyID { get; set; }
        public long OldHrUserHeadID { get; set; }
        public long NewHrUserHeadID { get; set; }
    }
}
