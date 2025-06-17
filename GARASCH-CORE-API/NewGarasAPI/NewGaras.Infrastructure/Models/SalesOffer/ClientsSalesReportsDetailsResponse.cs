using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class ClientsSalesReportsDetailsResponse
    {
        long filteredClientId;
        string filteredClientName;
        int filteredMonth;
        int filteredYear;
        long filteredSalesPersonId;
        int filteredBranchId;
        long filteredReportCreator;
        List<CrmSalesClientReport> salesReports;
        PaginationHeader paginationHeader;
        bool result;
        List<Error> errors;

        [DataMember]
        public long FilteredClientId
        {
            get
            {
                return filteredClientId;
            }

            set
            {
                filteredClientId = value;
            }
        }

        [DataMember]
        public string FilteredClientName
        {
            get
            {
                return filteredClientName;
            }

            set
            {
                filteredClientName = value;
            }
        }

        [DataMember]
        public int FilteredMonth
        {
            get
            {
                return filteredMonth;
            }

            set
            {
                filteredMonth = value;
            }
        }
        [DataMember]

        public int FilteredYear
        {
            get
            {
                return filteredYear;
            }

            set
            {
                filteredYear = value;
            }
        }

        [DataMember]
        public long FilteredSalesPersonId
        {
            get
            {
                return filteredSalesPersonId;
            }

            set
            {
                filteredSalesPersonId = value;
            }
        }

        [DataMember]
        public int FilteredBranchId
        {
            get
            {
                return filteredBranchId;
            }

            set
            {
                filteredBranchId = value;
            }
        }

        [DataMember]
        public List<CrmSalesClientReport> SalesReports
        {
            get
            {
                return salesReports;
            }

            set
            {
                salesReports = value;
            }
        }
        [DataMember]
        public long FilteredReportCreator
        {
            set { filteredReportCreator = value; }
            get { return filteredReportCreator; }
        }

        [DataMember]
        public PaginationHeader PaginationHeader
        {
            get
            {
                return paginationHeader;
            }

            set
            {
                paginationHeader = value;
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
