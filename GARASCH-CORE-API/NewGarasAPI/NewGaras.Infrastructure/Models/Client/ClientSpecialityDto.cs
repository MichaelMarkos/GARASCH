namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientSpecialityDto
    {
        public long? ID { get; set; }
        public int SpecialityID { get; set; }
        public string SpecialityName { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
    }
}