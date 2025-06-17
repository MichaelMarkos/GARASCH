using NewGaras.Infrastructure.DTO.ProjectSprint;
using NewGaras.Infrastructure.DTO.WorkFlow;
using NewGaras.Infrastructure.Models.TaskMangerProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.TaskMangerProject
{
    public class GetTaskMangerProjectDto
    {
        public long Id { get; set; }
        public string ProjectName { get; set; }
        public long ProjectCreatorID { get; set; }
        public string ProjectCreatorName { get; set; }
        public long? ClientID { get; set; }
        public string ClientName { get; set; }
        public long? ClientSalesPersonID { get; set; }
        public string ClientSalesPersonName { get; set; }
        public string ProjectDescription { get; set; } = null;

        public bool Active { get; set; }
        public decimal? Budget { get; set; }

        public int? CurrencyID { get; set; }

        public string CurrencyName { get; set; }
        //public List<string> AttachmentList { get; set; }
        public List<AttachmentList> AttachmentList { get; set; }
        public string ProjectLocation { get; set; }
        public long ContactPersonID { get; set; }
        public string ContactPersonEmail { get; set; }
        public string ContcatPersonHome { get; set; } 
        public string ContactpersonMobile { get; set; }
        public string ContcatPersonName { get; set; }
        public string ContcatPersonAddress { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool Billable { get; set; } 
        public int? PriortyID { get; set; }
        public string PriortyName { get; set; }
        public bool TimeTracking { get; set; }

        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public long? AreaID { get; set; }
        public string AreaName { get; set; }

        public int GovernorateID { get; set; }
        public string GovernorateName { get; set; }

        public bool Closed { get; set; }
        public bool Revision { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public int CostTypeID { get; set; }
        public string CostTypeName { get; set; }
        public int NumOfOpenTask { get; set; }
        public int NumOfClosedTask { get; set; }
        public decimal? ProgressPercentage { get; set; }
        public string ProgressComment {  get; set; }
        public decimal workingHours { get; set; }
        public decimal Expenses { get; set; }
        public int NumberOfUsers { get; set; }
        public decimal? TotalSumTaskBudget { get; set; }
        public List<GetProjectsprintDto> ProjectSprints { get; set; }
        public List<GetWorkFlowDto> ProjectWorkFlows { get; set; }
    }
}
