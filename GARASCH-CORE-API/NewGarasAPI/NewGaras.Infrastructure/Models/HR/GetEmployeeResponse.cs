using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.HR
{
    public class GetEmployeeResponse
    {
        bool result;
        List<Error> errors;
        EmployeeInfoData employeeInfo;
        //PaginationHeader paginationHeader;
        //public class PaginationHeader
        //{
        //    // the number of the current page
        //    public int CurrentPage { get; set; }

        //    //the total number of pages
        //    public int TotalPages { get; set; }

        //    //the count of items per each page
        //    public int ItemsPerPage { get; set; }

        //    //total count of items in the query
        //    public int TotalItems { get; set; }
        //}

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

        [DataMember]
        public EmployeeInfoData EmployeeInfo
        {
            get
            {
                return employeeInfo;
            }

            set
            {
                employeeInfo = value;
            }
        }

    }

}
