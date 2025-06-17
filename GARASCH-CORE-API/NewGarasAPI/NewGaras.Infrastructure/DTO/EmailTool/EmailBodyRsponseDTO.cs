using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.EmailTool
{
    public class EmailBodyRsponseDTO
    {
        public List<GetEmailsListFromDBDto> emailsList { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems {  get; set; }

        public PaginationHeader PaginationHeader { get; set; }
    }
}
