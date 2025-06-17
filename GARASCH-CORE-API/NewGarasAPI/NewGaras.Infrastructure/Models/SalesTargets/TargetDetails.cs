using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets
{
    public class TargetDetails
    {
        public int Year { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal AchievedTargetAmount { get; set; }
        public string AchievedTargetPercentage { get; set; }
    }
}
