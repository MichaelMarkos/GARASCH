using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.ChurchAndPriest
{
    public class GetHrUserPriestHistoryDTO
    {
        public long ID { get; set; }
        public long HrUserID { get; set; }
        public long PriestID { get; set; }
        public string PriestName { get; set; }
        public bool IsCurrent { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Reason { get; set; }
    }
}
