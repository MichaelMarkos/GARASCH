using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family
{
    public class EditHrUserFamilyDTO
    {
        public long Id { get; set; }
        public long? HrUserID { get; set; }
        public long? FamilyID { get; set; }
        public bool? IsHeadOfFamily { get; set; }
        public bool? Active { get; set; }
    }
}
