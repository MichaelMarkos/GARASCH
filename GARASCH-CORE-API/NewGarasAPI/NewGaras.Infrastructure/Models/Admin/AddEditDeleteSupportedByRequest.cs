using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Admin
{
    public class AddEditDeleteSupportedByRequest
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public bool? Active { get; set; }

        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
