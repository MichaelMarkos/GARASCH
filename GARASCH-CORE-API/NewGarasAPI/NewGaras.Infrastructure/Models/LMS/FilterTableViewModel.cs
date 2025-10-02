

namespace NewGaras.Infrastructure.Models.LMS
{
    public class FilterTableViewModel
    {
        [FromHeader]
        public long? userId { get; set; }
        [FromHeader]
        public DateTime? StartTime { get; set; }
        [FromHeader]
        public DateTime? TimeToday { get; set; }
        [FromHeader]

        public DateTime? EndTime { get; set; }
        [FromHeader]

        public int? CompetitionId { get; set; } = 0;
        [FromHeader]

        public int? SpecialDeptId { get; set; } = 0;
        [FromHeader]

        public int? LevelId { get; set; } = 0;
        //public string CompetitionName { get; set; }
        // public string SpecialDeptName { get; set; }
        //public string LevelName { get; set; }
        //public string DeptName { get; set; }
        [FromHeader]

        public int? HallId { get; set; } = 0;
        [FromHeader]

        public int TypeId { get; set; }
    }
}
