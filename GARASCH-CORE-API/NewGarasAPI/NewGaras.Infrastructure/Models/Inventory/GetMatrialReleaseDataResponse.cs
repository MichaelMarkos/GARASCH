using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetMatrialReleaseDataResponse
    {
        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }

        public List<MatrialReleaseDataVM> MatrialReleaseDataVM { get; set; }
    }
}
