using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.SalesOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetClientData

    {
        ClientData clientsData;
        ClientConsultantData clientConsultantData;
        List<GetClientAddress> clientAddressData;
        List<GetClientContactPerson> clientContactPersonData;
        List<GetClientMobile> clientMobileData;
        List<GetClientLandLine> clientLandLineData;
        List<GetClientFax> clientFaxData;
        List<GetClientPaymentTerm> clientPaymentTermData;
        List<GetClientBankAccount> clientBankAccountData;
        List<GetClientSalesPersonHistory> clientSalesPersonHistory;

        List<Attachment> licenseAttachements;
        List<Attachment> bussinessCardsAttachments;
        List<Attachment> brochuresAttachments;
        Attachment taxCardAttachment;
        Attachment commercialRecordAttachment;
        List<Attachment> otherAttachments;

        bool? isExistClientProfile;
        bool result;
        List<Error> errors;

        [DataMember]
        public ClientData ClientsData
        {
            get
            {
                return clientsData;
            }

            set
            {
                clientsData = value;
            }
        }

        [DataMember]
        public ClientConsultantData ClientConsultantData
        {
            get
            {
                return clientConsultantData;
            }

            set
            {
                clientConsultantData = value;
            }
        }

        [DataMember]
        public List<GetClientAddress> ClientAddressData
        {
            get
            {
                return clientAddressData;
            }

            set
            {
                clientAddressData = value;
            }
        }

        [DataMember]
        public List<GetClientContactPerson> ClientContactPersonData
        {
            get
            {
                return clientContactPersonData;
            }

            set
            {
                clientContactPersonData = value;
            }
        }

        [DataMember]
        public List<GetClientMobile> ClientMobileData
        {
            get
            {
                return clientMobileData;
            }

            set
            {
                clientMobileData = value;
            }
        }

        [DataMember]
        public List<GetClientLandLine> ClientLandLineData
        {
            get
            {
                return clientLandLineData;
            }

            set
            {
                clientLandLineData = value;
            }
        }

        [DataMember]
        public List<GetClientFax> ClientFaxData
        {
            get
            {
                return clientFaxData;
            }

            set
            {
                clientFaxData = value;
            }
        }

        [DataMember]
        public List<GetClientPaymentTerm> ClientPaymentTermData
        {
            get
            {
                return clientPaymentTermData;
            }

            set
            {
                clientPaymentTermData = value;
            }
        }

        [DataMember]
        public List<GetClientBankAccount> ClientBankAccountData
        {
            get
            {
                return clientBankAccountData;
            }

            set
            {
                clientBankAccountData = value;
            }
        }

        [DataMember]
        public List<GetClientSalesPersonHistory> ClientSalesPersonHistory
        {
            get
            {
                return clientSalesPersonHistory;
            }

            set
            {
                clientSalesPersonHistory = value;
            }
        }

        [DataMember]
        public List<Attachment> LicenseAttachements
        {
            get
            {
                return licenseAttachements;
            }

            set
            {
                licenseAttachements = value;
            }
        }

        [DataMember]
        public List<Attachment> BussinessCardsAttachments
        {
            get
            {
                return bussinessCardsAttachments;
            }

            set
            {
                bussinessCardsAttachments = value;
            }
        }

        [DataMember]
        public List<Attachment> BrochuresAttachments
        {
            get
            {
                return brochuresAttachments;
            }

            set
            {
                brochuresAttachments = value;
            }
        }

        [DataMember]
        public Attachment TaxCardAttachment
        {
            get
            {
                return taxCardAttachment;
            }

            set
            {
                taxCardAttachment = value;
            }
        }

        [DataMember]
        public Attachment CommercialRecordAttachment
        {
            get
            {
                return commercialRecordAttachment;
            }

            set
            {
                commercialRecordAttachment = value;
            }
        }

        [DataMember]
        public List<Attachment> OtherAttachments
        {
            get
            {
                return otherAttachments;
            }

            set
            {
                otherAttachments = value;
            }
        }

        [DataMember]
        public bool? IsExistClientProfile
        {
            get
            {
                return isExistClientProfile;
            }
            set
            {
                isExistClientProfile = value;
            }
        }
        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }
        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }
    }
}
