﻿namespace NewGaras.Infrastructure.Models
{
    public class ConsultantAddress
    {
        public long? ID { get; set; }
        public int CountryID { get; set; }
        public string CountryName { get; set; }
        public int GovernorateID { get; set; }
        public string GovernorateName { get; set; }
        public string Address { get; set; }
        public string BuildingNumber { get; set; }
        public string Floor { get; set; }
        public string Description { get; set; }
    }
}