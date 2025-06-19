namespace NewGaras.Infrastructure.DTO.HrUser
{
    public class HrUserMobileDto
    {
        public long? ID { get; set; }

        public string MobileNumber { get; set; } = string.Empty;

        public bool Active { get; set; }
    }
}