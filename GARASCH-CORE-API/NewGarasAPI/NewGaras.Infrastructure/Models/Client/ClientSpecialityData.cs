using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientSpecialityData
    {
        public long ClientId { get; set; }
        public List<ClientSpecialityDto> ClientSpecialities { get; set; }
    }
}
