namespace NewGaras.Infrastructure.Models
{
    public class ClientConsultantData

    {
        long? id;
        long clientId;
        string consultantName;
        string createdBy;
        string modifiedBy;
        string company;
        string consultantFor;

        List<ConsultantAddress> consultantAddresses;
        List<ConsultantFax> consultantFaxes;
        List<ConsultantEmail> consultantEmails;
        List<ConsultantMobile> consultantMobiles;
        List<ConsultantLandLine> consultantLandLines;
        List<ConsultantSpeciality> consultantSpecialities;

        [DataMember]
        public long? Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        [DataMember]
        public long ClientId
        {
            get
            {
                return clientId;
            }

            set
            {
                clientId = value;
            }
        }

        [DataMember]
        public string ConsultantName
        {
            get
            {
                return consultantName;
            }

            set
            {
                consultantName = value;
            }
        }

        [DataMember]
        public string CreatedBy
        {
            get
            {
                return createdBy;
            }

            set
            {
                createdBy = value;
            }
        }

        [DataMember]
        public string ModifiedBy
        {
            get
            {
                return modifiedBy;
            }

            set
            {
                modifiedBy = value;
            }
        }

        [DataMember]
        public string Company
        {
            get
            {
                return company;
            }

            set
            {
                company = value;
            }
        }

        [DataMember]
        public string ConsultantFor
        {
            get
            {
                return consultantFor;
            }

            set
            {
                consultantFor = value;
            }
        }
        [DataMember]

        public List<ConsultantAddress> ConsultantAddresses
        {
            get
            {
                return consultantAddresses;
            }

            set
            {
                consultantAddresses = value;
            }
        }

        [DataMember]
        public List<ConsultantFax> ConsultantFaxes
        {
            get
            {
                return consultantFaxes;
            }

            set
            {
                consultantFaxes = value;
            }
        }

        [DataMember]
        public List<ConsultantEmail> ConsultantEmails
        {
            get
            {
                return consultantEmails;
            }

            set
            {
                consultantEmails = value;
            }
        }

        [DataMember]
        public List<ConsultantMobile> ConsultantMobiles
        {
            get
            {
                return consultantMobiles;
            }

            set
            {
                consultantMobiles = value;
            }
        }

        [DataMember]
        public List<ConsultantLandLine> ConsultantLandLines
        {
            get
            {
                return consultantLandLines;
            }

            set
            {
                consultantLandLines = value;
            }
        }

        [DataMember]
        public List<ConsultantSpeciality> ConsultantSpecialities
        {
            get
            {
                return consultantSpecialities;
            }

            set
            {
                consultantSpecialities = value;
            }
        }
    }
}