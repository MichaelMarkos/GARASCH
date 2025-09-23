using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Family
{
    public class AddFamilyDTO
    {
        public string FamilyName { get; set; }
        public int FamilyStatusID { get; set; }
        public long? ServantId { get; set; }
    }
}
