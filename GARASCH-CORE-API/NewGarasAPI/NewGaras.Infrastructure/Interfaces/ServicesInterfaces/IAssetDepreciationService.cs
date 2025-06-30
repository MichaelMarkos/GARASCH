using NewGaras.Infrastructure.DTO.AssetDepreciation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IAssetDepreciationService
    {
        public BaseResponseWithId<long> AddProductionUOM(AddProductionUOMDTO dto, long creatorID);

        public BaseResponseWithId<long> EditProductionUOM(EditProductionUOMDTO dto, long userID);
        public SelectDDLResponse GetProductionUOMDDL();
        public BaseResponseWithId<long> AddDepreciationType(AddProductionUOMDTO dto, long creatorID);
        public BaseResponseWithId<long> EditDepreciationType(EditProductionUOMDTO dto, long userID);
        public SelectDDLResponse GetDepreciationTypeDDL();
        public BaseResponseWithId<long> AddAssetDepreciation(AddAssetDepreciationDTO dto, long creatorID);
        public BaseResponseWithId<long> EditAssetDepreciation(EditAssetDepreciationDTO dto, long userID);
        public BaseResponseWithData<List<GetAssetDepreciationDTO>> GetAssetDepreciation(GetAssetDepreciationFilters filters);
    }
}
