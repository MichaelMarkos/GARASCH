using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EInvoice.App_code.Oracle.Model
{
    public class IssuerModel
    {
        string companyName;
        string companyNameAR;
        string regNum;
        string type;
        string branchID;
        string country;
        string governate;
        string regionCity;
        string buildNum;
        string postalCode;
        string floor;
        string room;
        string landmark;
        string addInfo;
        string street;
        string activityCode1;
        string activityCode2;
        string activityCode3;
        string activiyDesc1;
        string activiyDesc2;
        string activiyDesc3;

        public IssuerModel(string companyName, string companyNameAR, string regNum, string type, string branchID, string country, string governate, string regionCity, string buildNum, string postalCode, string floor, string room, string landmark, string addInfo, string street, string activityCode1, string activityCode2, string activityCode3, string activiyDesc1, string activiyDesc2, string activiyDesc3)
        {
            this.companyName = companyName;
            this.companyNameAR = companyNameAR;
            this.regNum = regNum;
            this.type = type;
            this.branchID = branchID;
            this.country = country;
            this.governate = governate;
            this.regionCity = regionCity;
            this.buildNum = buildNum;
            this.postalCode = postalCode;
            this.floor = floor;
            this.room = room;
            this.landmark = landmark;
            this.addInfo = addInfo;
            this.street = street;
            this.activityCode1 = activityCode1;
            this.activityCode2 = activityCode2;
            this.activityCode3 = activityCode3;
            this.activiyDesc1 = activiyDesc1;
            this.activiyDesc2 = activiyDesc2;
            this.activiyDesc3 = activiyDesc3;
        }

        public string CompanyName
        {
            get
            {
                return companyName;
            }

            set
            {
                companyName = value;
            }
        }

        public string CompanyNameAR
        {
            get
            {
                return companyNameAR;
            }

            set
            {
                companyNameAR = value;
            }
        }

        public string RegNum
        {
            get
            {
                return regNum;
            }

            set
            {
                regNum = value;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public string BranchID
        {
            get
            {
                return branchID;
            }

            set
            {
                branchID = value;
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

        public string BuildNum
        {
            get
            {
                return buildNum;
            }

            set
            {
                buildNum = value;
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

        public string AddInfo
        {
            get
            {
                return addInfo;
            }

            set
            {
                addInfo = value;
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

        public string ActivityCode1
        {
            get
            {
                return activityCode1;
            }

            set
            {
                activityCode1 = value;
            }
        }

        public string ActivityCode2
        {
            get
            {
                return activityCode2;
            }

            set
            {
                activityCode2 = value;
            }
        }

        public string ActivityCode3
        {
            get
            {
                return activityCode3;
            }

            set
            {
                activityCode3 = value;
            }
        }

        public string ActiviyDesc1
        {
            get
            {
                return activiyDesc1;
            }

            set
            {
                activiyDesc1 = value;
            }
        }

        public string ActiviyDesc2
        {
            get
            {
                return activiyDesc2;
            }

            set
            {
                activiyDesc2 = value;
            }
        }

        public string ActiviyDesc3
        {
            get
            {
                return activiyDesc3;
            }

            set
            {
                activiyDesc3 = value;
            }
        }
    }
}
