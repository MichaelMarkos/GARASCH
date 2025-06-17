namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class EInvoiceSettingData
    {
        public int? ID { get; set; }
        public string ClientIdProduction { get; set; }
        public string Clientsecret1Production { get; set; }
        public string Clientsecret2Production { get; set; }
        public string ClientIdTest { get; set; }
        public string Clientsecret1Test { get; set; }
        public string Clientsecret2Test { get; set; }
        public string RegistrationNumber { get; set; }
        public string IssuerType { get; set; }
        public string ActivityCode1 { get; set; }
        public string ActivityCode2 { get; set; }
        public string ActivityCode3 { get; set; }
        public string ItemCodeType { get; set; }
        public string IssureName { get; set; }
        public string Path { get; set; }
        public string CompanyName { get; set; }
        public string PinCode { get; set; }
        public bool Active { get; set; }
        public bool? IsSignature { get; set; }
        public bool? IsProduction { get; set; }
        public string ItemTypeEINVOICE { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}