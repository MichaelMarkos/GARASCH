using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientFaxDataDto
    {
        public long ClientId { get; set; }
        public List<ClientFaxData> ClientFaxes { get; set; }
    }
}
