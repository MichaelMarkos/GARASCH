namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class ClientBaseData
    {
        public long? id {  get; set; }
        public string idEnc { get; set; }
        public string name {  get; set; }
        public string logo { get; set; }
        public bool? hasLogo { get; set; }
        public int maintenanceForCount { get; set; }

        public int NeedApproval { get; set; }
    }
}