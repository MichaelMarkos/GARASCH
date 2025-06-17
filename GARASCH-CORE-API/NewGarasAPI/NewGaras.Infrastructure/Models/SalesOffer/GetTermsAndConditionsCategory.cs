using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetTermsAndConditionsCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<GetTermsAndConditions> TermsAndConditionsList { get; set; }
    }
}