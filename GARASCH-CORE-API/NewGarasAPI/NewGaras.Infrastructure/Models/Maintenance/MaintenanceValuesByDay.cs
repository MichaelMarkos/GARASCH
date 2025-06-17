namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceValuesByDay : NearestClientVisitMaintenanceDetails
    {
        public bool? Status { get; set; }
        public decimal? Evaluation { get; set; }
        public decimal? ClientSatisfactionRate { get; set; }
        public decimal? CollectedAmount { get; set; }
        public bool WithContract { get; set; }
        public string lastVisitDate { get; set; }
        public int remainVisitsNo { get; set; }
        public long? ManagementMaintenanctOrderID { get; set; }

        public List<Attachment> MaintenanceProblemAttachments { get; set; }
        public string InventoryItemName { get; set; }

        public bool ClientIsBlocked { get; set; } = false;
    }
}