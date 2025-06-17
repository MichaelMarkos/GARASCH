using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectManagement
{
    public class DeliveryTypeDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
    }
}
