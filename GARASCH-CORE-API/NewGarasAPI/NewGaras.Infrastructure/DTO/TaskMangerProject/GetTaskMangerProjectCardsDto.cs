using NewGaras.Infrastructure.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskMangerProject
{
    public class GetTaskMangerProjectCardsDto
    {
        public long ID { get; set; }
        public string ProjectName { get; set; }
        public int NumOfOpenTasks { get; set; }
        public int NumOfClosedTasks { get; set; }
        public string EndDate { get; set; }
        public long RemainDays { get; set; }
        public int? PriortyID { get; set; }
        public string PriortyName { get; set; }
        public decimal TotalCost { get; set; }
        public long TotalHours { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public List<UserWithJobTitleDDL> UsersList { get; set; }
    }
}
