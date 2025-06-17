using NewGarasAPI.Models.Project.UsedInResponses;


namespace NewGarasAPI.Models.ProjectsDetails.Responses
{
    public class GeneralInfoAndProjectHistoryResponse
    {
        //------------------data from project table---------------
        [DataMember]
        public bool result {  get; set; }

        [DataMember]
        public List<Error> errors { get; set; }


        [DataMember]
        public long ProjectId { get; set; }

        [DataMember]
        public long? ProjectManagerId { get; set; }

        [DataMember]
        public string StartDate { get; set; }

        [DataMember]
        public string EndDate { get; set; }

        [DataMember]
        public string status { get; set; }

        [DataMember]
        public string ProjectSerial { get; set; }

        [DataMember]
        public byte[] SerialQR {  get; set; }

        //------------------dataf from salesoffer table----------
        [DataMember]
        public string Note { get; set; }

        [DataMember]
        public string ProjectName {get; set; }

        [DataMember]
        public string ProjectLocation { get; set; }

        [DataMember]
        public string ContactPersonName { get; set; }

        [DataMember]
        public string ContactPersonEmail { get; set; }

        [DataMember]
        public string ContactPersonMobile { get; set; }

        [DataMember]
        public string  OfferSerial { get; set; }

        [DataMember]
        public string OfferType { get; set; }

        [DataMember]
        public string OfferLink { get; set; }

        //--------------------data from user table---------------------------
        [DataMember]
        public long   SelasRepId { get; set; }  

        [DataMember]
        public string SelasRepFullName { get; set; }            //get the (firstname . middlename and the lastname and join them togther) from user table

        //-------------------data from salesOfferAttachment------------------
        [DataMember]
        public string ContractAttachmentPath { get; set; }

        [DataMember]
        public string OfferAttachmentPath { get; set; }

        //----------------------fro testing--------------------------------
        [DataMember]
        public List<ProductVM> ProductList  { get; set; }

    }
}
