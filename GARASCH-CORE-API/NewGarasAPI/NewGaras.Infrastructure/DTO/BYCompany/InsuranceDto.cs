namespace NewGaras.Infrastructure.DTO.BYCompany
{
    public class InsuranceDto
    {
        public long? Id { get; set; }
        public long UserPatientId { get; set; }
        public string Name { get; set; }
        public string IncuranceNo { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool? Active { get; set; } = true;
    }
}