using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Library;

namespace NewGaras.Infrastructure.Interfaces.Library
{
    public interface ILibraryService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithData<List<GetBorrowedBookData>> GetBorrowedBookData();
        public BaseResponseWithData<GetBorrowedBookForEachUserList> GetBorrowedBookForEachUser();
        public BaseResponseWithData<GetBooksDashboardData> GetBooksDashboardData();
        public BaseResponseWithData<GetBorrowedBooksList> GetBorrowedBooksList();
    }
}
