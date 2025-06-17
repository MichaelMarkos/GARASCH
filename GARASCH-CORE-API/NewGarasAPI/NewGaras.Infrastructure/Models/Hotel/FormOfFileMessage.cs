using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Hotel
{
    public class FormOfFileMessage
    {
        public string chatId { get; set; }
        public string? caption { get; set; }
        public IFormFile file { get; set; }
        public string? fileName { get; set; }
        public string Name { get; set; }
        public string? NameLab { get; set; }
        //public string? PdfUrl { get; set; }
        public decimal? Cost { get; set; }


    }
    public class ApiResponse
    {
        public string idMessage { get; set; }
        public string urlFile { get; set; }
    }
}
