using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class UpdateClientClassRequest
    {
        public long ClientId { get; set; }
        public int ClassificationId { get; set; }
    }
}
