namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class ProjectLocationDetails
    {
        public int? CountryId { get; set; }
        public int? CityId { get; set; }
        public long? AreaId { get; set; }
        public string BuildingNumber { get; set; }
        public string Floor { get; set; }
        public string Street { get; set; }
        public string Description { get; set; }
        public decimal? LocationX { get; set; }
        public decimal? LocationY { get; set; }
    }
}