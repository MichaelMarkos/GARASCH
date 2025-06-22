using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family
{
    public class EditHrUserFamilyActiveDTO
    {
        public long HruserId { get; set; }
        public long FamilyID { get; set; }
        public bool Active { get; set; }
    }
}
