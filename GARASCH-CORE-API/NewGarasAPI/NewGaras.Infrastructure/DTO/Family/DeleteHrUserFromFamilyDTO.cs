using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family
{
    public class DeleteHrUserFromFamilyDTO
    {
        public long FamilyID { get; set; }
        public long HrUserID { get; set; }
    }
}
