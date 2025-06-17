namespace NewGaras.Infrastructure.Models.Inventory
{
    public class MainMatrialReleaseDataVM
    {
        public string shippingMethod { get; set; }
        public string mainAddress { get; set; }
        public string mainContactName { get; set; }
        public string mainContactMobile { get; set; }
        public long SalesOfferID { get; set; }
        public long? ClientID { get; set; }
        public long? ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string OfferPricingComment { get; set; }
        public string OfferProjectData { get; set; }
        public string OfferTechnicalInfo { get; set; }

        public List<ClientAddressListVM> clientAddressList { get; set; }
        public List<ClientContactVM> clientContactList { get; set; }
    }
}