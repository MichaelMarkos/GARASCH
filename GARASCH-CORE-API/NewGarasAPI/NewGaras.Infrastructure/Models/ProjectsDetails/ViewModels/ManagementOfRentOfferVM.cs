using System.ComponentModel.DataAnnotations;

namespace NewGarasAPI.Models.ProjectsDetails.ViewModels
{
    public class ManagementOfRentOfferVM
    {
        [DataMember] 
        public long OfferID { get; set; }
        [DataMember] 
        public int ProjectID { get; set; }
        [DataMember] 
        public long ManagementOfRentOrderID { get; set; }
        [DataMember] 
        public string ReleaseDate { get; set; }
        [DataMember] 
        public string PlannedReceivingDate { get; set; }
        [DataMember] 
        public string ActualReceivingDate { get; set; }
        
        [DataMember] 
        public decimal CostOfExtraDays { get; set; }
        //[DataMember] 
        //public decimal TotalCostOfExtraDays { get; set; }
        [DataMember] 
        public decimal ExtraRequired { get; set; }
        [DataMember] 
        public decimal Discount { get; set; }
        [DataMember] 
        public decimal TotalRequiredExtraCost { get; set; }
        [DataMember] 
        public bool DamageOrPenaltiesStatus { get; set; }
        [DataMember] 
        public string DamageOrPenaltiesDesc { get; set; }
        [DataMember] 
        public decimal DamageOrPenaltiesCost { get; set; }
        [DataMember] 
        public bool FinFeedBackConfirmed { get; set; }
        [DataMember] 
        public string FinFeedBackReplyDate { get; set; }
        [DataMember] 
        public long FinFeedBackUserID { get; set; }
        [DataMember] 
        public string FinFeedBackUserName { get; set; }
        [DataMember] 
        public bool RentStatus { get; set; }

        [DataMember]
        public List<IFormFile> RentAttachment { get; set; }

        [DataMember]
        public string AttachmentCategory { get; set; } = "DamageOrPenaltiesAttach";

        [DataMember]
        public string AttachmentDescription { get; set; } = "DamageOrPenaltiesAttach";
    }
}
