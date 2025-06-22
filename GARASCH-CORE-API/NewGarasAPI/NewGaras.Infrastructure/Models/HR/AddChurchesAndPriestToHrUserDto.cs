using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.HR
{
    public class AddChurchesAndPriestToHrUserDto
    {
        public long HrUserId { get; set; }
        public long? ChurchOfPresenceId { get; set; }
        public long? BelongToChurchId { get; set; }
        public long? PriestId { get; set; }

        public string Reason { get; set; }
    }
}
