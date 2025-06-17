using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class SalesOfferIDAndProjectID
    {
        long salesOfferID;
        long projectID;
        bool result;
        List<Error> errors;


        [DataMember]
        public long SalesOfferID
        {
            get
            {
                return salesOfferID;
            }

            set
            {
                salesOfferID = value;
            }
        }
        [DataMember]
        public long ProjectID
        {
            get
            {
                return projectID;
            }

            set
            {
                projectID = value;
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
