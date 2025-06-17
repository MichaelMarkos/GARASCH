namespace NewGaras.Infrastructure.Models.Inventory
{
    public class MatrialReleaseDataVM
    {
        public string shippingMethod { get; set; }
        public string mainAddress { get; set; }
        public string mainContactName { get; set; }
        public string mainContactMobile { get; set; }
        public long? ProjectID { get; set; }
        public string Comment { get; set; }

    }
}