using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class BaseResponseWithDataPaging<ViewModel>
    {
        public bool Result { get; set; }
        public List<string> Errors { get; set; }
        //public List<Error> errors { get; set; }
        public ViewModel Data { get; set; }

        public int? NoOfItems { get; set; }
        public int? PageNo { get; set; }
        public int? TotalPages { get; set; }
        public int? TotalItems { get; set; }

    }

    //public class BaseResponseWithDataAndHeader<ViewModel>
    //{
    //    public bool Result { get; set; }
    //    public List<string> Errors { get; set; }
    //    public ViewModel Data { get; set; }

    //    public PaginationHeader PaginationHeader { get; set; }
    //}
}
