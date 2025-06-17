using NewGaras.Infrastructure.Models.Inventory;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class AddVisitsScheduleOfMaintenance
    {
        public long ID { get; set; }
        public long? ManagementOfMaintenanceOrderID { get; set; }
        public string Serial { get; set; }
        public string PlannedDate { get; set; }
        public string VisitDate { get; set; }
        public string ConfirmedDate { get; set; }
        public string ClientProblem { get; set; }
        public string MaintenanceVisitType { get; set; }
        public bool Status { get; set; }
        public long? MaintenanceForID { get; set; }
        public long? AssignedToID { get; set; }
        public string AssignedToName { get; set; }
        public string DepartmentName { get; set; }
        public List<AttachmentFile> MaintenanceProblemAttachments { get; set; }
    }
}