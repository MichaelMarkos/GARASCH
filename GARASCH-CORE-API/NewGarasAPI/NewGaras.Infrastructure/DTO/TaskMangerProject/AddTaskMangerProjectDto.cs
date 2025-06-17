using NewGaras.Infrastructure.Models.TaskMangerProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskMangerProject
{
    public class AddTaskMangerProjectDto
    {
        public string ProjectName { get; set; }

        public long ClientID { get; set; }

        public string ProjectDescription { get; set; } = null;

        public decimal? Budget { get; set; }

        public int? CurrencyID { get; set; }

        public List<IFormFile> AttachmentList { get; set; }

        public int BranchId { get; set; }
        public string ProjectLocation { get; set; }
        public long ContactPersonID { get; set; }
        
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool? Billable { get; set; } = false;
        public int? PriorityID { get; set; }
        public bool? TimeTracking { get; set; } = false ;

        public bool? Active { get; set; }

        public int? costTypeID { get; set; }

        public List<AddAttachment> AttachmentListTest{ get; set; } = new List<AddAttachment>();

        //---------------------------project Contact Person table -----------------

        public string ProjectContactPersonName { get; set; }
        public string ProjectContactPersonMob { get; set; }
        public string ProjectContactPersonHome { get; set; } = null;
        public string ProjectContactPersonEmail { get; set; } = null;
        public string ProjectContactPersonAddress { get; set; }

        public int? ProjectContactPersonCountryId { get; set; }

        public int? ProjectContactPersonGovernorateID { get; set; }

        public long? ProjectContactPersonAreaId { get; set; }
    }
}
