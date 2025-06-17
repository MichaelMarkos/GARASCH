using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.JobTitle
{
    public class EditJobTitleDto
    {
        public long Id { get; set; }
        public string JobTitleName { get; set; }

        public decimal? hourlyRate { get; set; }

        public int currency { get; set; }
        public string Description { get; set; }
    }
}
