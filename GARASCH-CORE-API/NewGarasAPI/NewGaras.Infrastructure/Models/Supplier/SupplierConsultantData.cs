namespace NewGaras.Infrastructure.Models.Supplier
{
    public class SupplierConsultantData
    {
        public long? Id { get; set; }
        public long SupplierId { get; set; }
        public string ConsultantName { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Company {  get; set; }
        public string ConsultantFor { get; set; }

        public List<ConsultantAddress> ConsultantAddresses { get; set; }
        public List<ConsultantFax> ConsultantFaxes { get; set; }
        public List<ConsultantEmail> ConsultantEmails { get; set; }
        public List<ConsultantMobile> ConsultantMobiles { get; set; }
        public List<ConsultantLandLine> ConsultantLandLines { get; set; }
        public List<ConsultantSpeciality> ConsultantSpecialities { get; set; }
    }
}