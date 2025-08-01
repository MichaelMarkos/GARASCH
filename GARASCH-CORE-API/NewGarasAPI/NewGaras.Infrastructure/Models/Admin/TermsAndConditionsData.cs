﻿namespace NewGarasAPI.Models.Admin
{
    public class TermsAndConditionsData
    {
        public int? ID { get; set; }
        public string Name { get; set; }
        public string TermsAndConditionsName { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public string CreatedBy { get; set; }
        public long CreatedById { get; set; }
        public string ModifiedBy { get; set; }
        public long? ModifiedById { get; set; }
        public int TermsCategoryID { get; set; }
    }
}