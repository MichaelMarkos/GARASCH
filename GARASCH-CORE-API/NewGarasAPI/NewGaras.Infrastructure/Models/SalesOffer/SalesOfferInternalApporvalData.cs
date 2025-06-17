namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class SalesOfferInternalApporvalData
    {
        public long? Id { get; set; }
        public long SalesOfferId { get; set; }
        public string Type { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public long? GroupId { get; set; }
        public string GroupName { get; set; }
        public string ByUserId { get; set; }
        public string ByUserName { get; set; }
        public string Reply { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? Active { get; set; }
    }
}