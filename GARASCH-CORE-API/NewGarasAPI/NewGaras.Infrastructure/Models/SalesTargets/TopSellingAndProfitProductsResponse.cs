using NewGaras.Infrastructure.Models.SalesTargets.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesTargets
{
    public class TopSellingAndProfitProductsResponse
    {
        List<TopSellingProfitProcuct> topSellingProducts;
        List<TopSellingProfitProcuct> topProfitProducts;
        bool result;
        List<Error> errors;

        [DataMember]
        public List<TopSellingProfitProcuct> TopSellingProducts
        {
            get
            {
                return topSellingProducts;
            }

            set
            {
                topSellingProducts = value;
            }
        }
        [DataMember]
        public List<TopSellingProfitProcuct> TopProfitProducts
        {
            get
            {
                return topProfitProducts;
            }

            set
            {
                topProfitProducts = value;
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
