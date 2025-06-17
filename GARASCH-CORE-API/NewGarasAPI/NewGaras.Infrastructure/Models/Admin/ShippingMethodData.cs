namespace NewGarasAPI.Models.Admin
{
    public class ShippingMethodData
    {
        public long? ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}
