using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class GetMatrialReleaseIndexListFilters
    {
        [FromHeader] 
        public long? StoreKeeperUserId {  get; set; } 
        [FromHeader]
        public int? StoreId { get; set; }
        [FromHeader] 
        public List<string> StatusList { get; set; }
        [FromHeader] 
        public string FirstName { get; set; } 
        [FromHeader]
        public string LastName { get; set; } 
        [FromHeader]
        public string InsuranceNum { get; set; } 
        [FromHeader] 
        public string ReleaseDate { get; set; }

        [FromHeader]
        public int CurrentPage { get; set; } = 1;

        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
    }
}
