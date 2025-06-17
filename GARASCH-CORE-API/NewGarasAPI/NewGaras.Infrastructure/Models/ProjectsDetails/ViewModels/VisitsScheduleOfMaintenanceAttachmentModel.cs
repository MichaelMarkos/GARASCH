



namespace NewGarasAPI.Models.ProjectsDetails.ViewModels
{
    public class VisitsScheduleOfMaintenanceAttachmentModel
    {
        [DataMember]
        public long AttachmentId { get; set; }

        [DataMember]
        public long ManagementOfMaintenanceOrderId { get; set; }

        [DataMember]
        public long VisitsScheduleOfMaintenanceId { get; set; }

        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public IFormFile VisitsScheduleAttachment { get; set; }
    }
}
