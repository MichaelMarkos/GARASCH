using NewGaras.Infrastructure.DTO.HrUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.JobTitle
{
    public  class GetJobTilteDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? HourlyRate { get; set; }

        public int? Currency { get; set; }
        public string CurrencyName { get; set; }
        public string Description { get; set; }

        public List<HrUsersWithJobTitleNameImage> usersInJobTile { get; set; }
    }
}
