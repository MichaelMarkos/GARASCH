using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.RoyalTent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IRoyalTentService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public FileInfo GetFileRandom();

        public Task<BaseMessageExcelResponse> RoyalTelesqupUmbrellaCalculator(string Paint, string Sales, string Size, string Cloth, string Fronton);

        public Task<BaseMessageExcelResponse> RoyalTelesqupUmbrellaExcel(RoyalTelesqupUmbrellaFilters filters);

        public Task<BaseMessageMainVariablesExcelResponse> MainVariablesRoyalTentExcel(MainVariablesRoyalTentFilters filters);

        public BaseResponse UpdateRoyalTentExcel(Stream stream);
    }
}
