using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class AddHrUserAddessList
    {
        public List<HrUserAddressDto> Addresses { get; set; } = new List<HrUserAddressDto>();
    }
}
