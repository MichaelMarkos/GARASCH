namespace NewGaras.Infrastructure.Models
{
    public class ClientMainData
    {
        public long? Id { get; set; }
        public string IdEnc { get; set; }
        public string Name { get; set; }
        public string EnglishName { get; set; }
        public long SalesPersonID { get; set; }
        public long? ApprovedById { get; set; }
        public string ApprovedByName { get; set; }
        public string SalesPersonName { get; set; }
        public string SalesPersonLogo { get; set; }
        public IFormFile Logo { get; set; }
        public bool? HasLogo { get; set; }
        public string LogoURL { get; set; }
        public string Status { get; set; }
        public string ExpirationDate { get; set; }
        public bool IsExpired { get; set; }
        public int RemainingDays { get; set; }
        public string RegistrationDate { get; set; }
        public decimal RemainCollection {  get; set; }
        public decimal Volume {  get; set; }
        public long? ClientSerialCounter { get; set; }
        public string ClientSerial {  get; set; }
        public int? ClassificationId { get; set; }
        public string ClassificationName { get; set; }
        public string ClassificationComment { get; set; }
    }
}