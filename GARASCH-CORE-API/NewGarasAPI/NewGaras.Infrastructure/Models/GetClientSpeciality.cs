namespace NewGaras.Infrastructure.Models
{
    public class GetClientSpeciality
    {
        public long? ID { get; set; }
        public int SpecialityID { get; set; }
        public string SpecialityName { get; set; }
        public bool Active { get; set; }
    }
}