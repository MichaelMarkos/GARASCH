namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class TermsAndConditions
    {
        public long Id { get; set; }
        public int TermsCategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}