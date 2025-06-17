using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model
{
    [Serializable]
    public class CustomerModel
    {

        private string accountNumber;
        private string partyName;
        private string country;
        private string street;
        private string regionCity;
        private string governate;
        private string buildingNumber;
        private string postalCode;
        private string floor;
        private string room;
        private string landmark;
        private string additionalInformation;
        private string cusType;
        private string taxpayerCode;

        public string TaxpayerCode
        {
            get
            {
                return taxpayerCode;
            }

            set
            {
                taxpayerCode = value;
            }
        }
        public string AccountNumber
        {
            get
            {
                return accountNumber;
            }

            set
            {
                accountNumber = value;
            }
        }

        public string PartyName
        {
            get
            {
                return partyName;
            }

            set
            {
                partyName = value;
            }
        }

        public string Country
        {
            get
            {
                return country;
            }

            set
            {
                country = value;
            }
        }

        public string Street
        {
            get
            {
                return street;
            }

            set
            {
                street = value;
            }
        }

        public string RegionCity
        {
            get
            {
                return regionCity;
            }

            set
            {
                regionCity = value;
            }
        }

        public string Governate
        {
            get
            {
                return governate;
            }

            set
            {
                governate = value;
            }
        }

        public string BuildingNumber
        {
            get
            {
                return buildingNumber;
            }

            set
            {
                buildingNumber = value;
            }
        }

        public string PostalCode
        {
            get
            {
                return postalCode;
            }

            set
            {
                postalCode = value;
            }
        }

        public string Floor
        {
            get
            {
                return floor;
            }

            set
            {
                floor = value;
            }
        }

        public string Room
        {
            get
            {
                return room;
            }

            set
            {
                room = value;
            }
        }

        public string Landmark
        {
            get
            {
                return landmark;
            }

            set
            {
                landmark = value;
            }
        }

        public string AdditionalInformation
        {
            get
            {
                return additionalInformation;
            }

            set
            {
                additionalInformation = value;
            }
        }

        public string CusType
        {
            get
            {
                return cusType;
            }

            set
            {
                cusType = value;
            }
        }

        public CustomerModel()
        {
            accountNumber = "";
            partyName = "";
            country = "";
            street = "";
            regionCity = "";
            governate = "";
            buildingNumber = "";
            postalCode = "";
            floor = "";
            room = "";
            landmark = "";
            additionalInformation = "";
            cusType = "";
            taxpayerCode = "";
        }
        public CustomerModel(string accountNumber, string partyName, string country, string street, string regionCity, string governate, string buildingNumber, string postalCode, string floor, string room, string landmark, string additionalInformation, string cusType, string taxpayerCode)
        {
            this.accountNumber = accountNumber;
            this.partyName = partyName;
            this.country = country;
            this.street = street;
            this.regionCity = regionCity;
            this.governate = governate;
            this.buildingNumber = buildingNumber;
            this.postalCode = postalCode;
            this.floor = floor;
            this.room = room;
            this.landmark = landmark;
            this.additionalInformation = additionalInformation;
            this.cusType = cusType;
            this.taxpayerCode = taxpayerCode;
        }
    }
}
