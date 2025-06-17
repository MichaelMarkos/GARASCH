using NewGaras.Infrastructure.Models.CRM.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.CRM
{
    [DataContract]
    public class GetSalesAndCRMReportStatiscsGroupbyDateResponse
    {
        List<ReportSalesAndCRMStatisctsPerDate> data;

        bool result;
        List<Error> errors;

        [DataMember]
        public List<ReportSalesAndCRMStatisctsPerDate> Data
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
