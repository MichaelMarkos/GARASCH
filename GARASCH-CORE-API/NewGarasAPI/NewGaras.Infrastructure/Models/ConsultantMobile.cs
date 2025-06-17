namespace NewGaras.Infrastructure.Models
{
    public class ConsultantMobile
    {
        public long? ID { get; set; }
        public string Mobile { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
    }
}