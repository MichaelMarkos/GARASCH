using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class GetChurchesAndPriestsToHrUser
    {
        public long HrUserId { get; set; }
        public long? ChurchOfPresenceId { get; set; }
        public string ChurchOfPresenceName { get; set; }

        public long? ChurchOfPresenceEparchyId { get; set; }
        public string ChurchOfPresenceEparchyName { get; set; }
        public long? BelongToChurchId { get; set; }
        public string BelongToChurchName { get; set; }
        public long? BelongToChurchEparchyId { get; set; }
        public string BelongToChurchEparchyName { get; set; }
        public long? PriestId { get; set; }
        public string PriestName { get; set; }
    }
}
