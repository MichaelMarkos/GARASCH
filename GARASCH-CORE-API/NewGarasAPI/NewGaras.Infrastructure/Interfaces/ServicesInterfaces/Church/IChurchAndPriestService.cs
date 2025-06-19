using NewGaras.Infrastructure.DTO.ChurchAndPriest;
using NewGaras.Infrastructure.DTO.ChurchAndPriest.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Church
{
    public interface IChurchAndPriestService
    {
        public BaseResponseWithId<long> AddNewPriest(AddNewPriestDTO dto, long userID);

        public BaseResponseWithId<long> EditPriest(EditPriestDTO dto, long userID);

        public BaseResponseWithData<List<GetPriestsListDTO>> GetPriestList(GetPriestsListFilters filters);
    }
}
