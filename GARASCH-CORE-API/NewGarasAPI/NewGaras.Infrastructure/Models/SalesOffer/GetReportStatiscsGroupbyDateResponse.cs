using NewGaras.Infrastructure.Models.SalesOffer.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    [DataContract]
    public class GetReportStatiscsGroupbyDateResponse
    {
        List<ReportStatisctsPerDate> data;

        bool result;
        List<Error> errors;

        [DataMember]
        public List<ReportStatisctsPerDate> Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
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
