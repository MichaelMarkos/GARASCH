using NewGarasAPI.Models.Project.UsedInResponses;

namespace NewGaras.Infrastructure.Models
{
    public class SalesPersonProducts
    {
        public long SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }

        public List<SellingProductsCRM> SalesPersonProductsList { get; set; }
    }
}