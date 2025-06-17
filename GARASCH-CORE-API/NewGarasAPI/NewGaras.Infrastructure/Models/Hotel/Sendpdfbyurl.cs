using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Hotel
{
    public class Sendpdfbyurl
    {
        public string chatId { get; set; }
        public string urlFile { get; set; }
        public string fileName { get; set; }
        public string caption { get; set; }
    }
}
