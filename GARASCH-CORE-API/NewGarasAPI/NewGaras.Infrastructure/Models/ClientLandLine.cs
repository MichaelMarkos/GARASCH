namespace NewGaras.Infrastructure.Models
{
    public class ClientLandLineData
    {
        public long? ID { get; set; }
        public string LandLine { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
    }
}