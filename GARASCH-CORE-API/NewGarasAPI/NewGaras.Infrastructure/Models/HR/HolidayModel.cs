using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Models.HR
{
    public class HolidayModel
    {
        public string HolidayEnName { get; set; }
        public string HolidayArName { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public int BranchId { get; set; }

        public bool IsWeekEnd { get; set; }
    }
}