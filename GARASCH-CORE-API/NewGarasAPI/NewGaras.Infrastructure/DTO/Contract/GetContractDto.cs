using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Contract
{
    public class GetContractDto
    {
        public int Id { get; set; }
        public int ContactTypeID { get; set; }
        public string ContactTypeName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public bool IsCurrent { get; set; }
        public bool IsAllowOverTime { get; set; } = false;
        public bool ISAutomatic { get; set; } = false;
        public bool? FreeWorkingHours { get; set; }
        public bool? AllowBreakDeduction { get; set; }

        public bool? AllowLocationTracking { get; set; }
        public float? Diameter { get; set; }
        public bool? AllowComissions { get; set; }
        public decimal? ComissionRate { get; set; }
        public decimal? ComissionPercentage { get; set; }
        public bool? AllowOvertime { get; set; }
        public bool? AutomaticPerWorkingHours { get; set; }
        public bool? AllowedWithApproval { get; set; }

        public long? HrUserId { get; set; }

        public int? ProbationPeriod { get; set; }

        public int? NoticedByEmployee { get; set; }

        public int? NoticedByCompany { get; set; }

        public long FirstReportTo { get; set; }
        public string FirstReportToName { get; set; }

        public long? SecondReportTo { get; set; }
        public string SecondReportToName { get; set; }
        public decimal? WorkingHours { get; set; }

    }
}
