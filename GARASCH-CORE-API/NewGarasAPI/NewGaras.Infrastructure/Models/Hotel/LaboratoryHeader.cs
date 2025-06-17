

namespace NewGaras.Infrastructure.Models.Hotel
{
    public class LaboratoryHeader
    {
        [FromHeader]
        public string Name { get; set; }
        [FromHeader]

        public string Mobile { get; set; }
        [FromHeader]
           
        public string NameLab { get; set; }
        [FromHeader]

        public string PdfUrl { get; set; }

        [FromHeader]

        public decimal? Cost { get; set; }
        [FromHeader]


        public bool? Result { get; set; }
        [FromHeader]


        public DateTime? Date { get; set; }
        [FromHeader]


        public DateTime? FromDate { get; set; }
        [FromHeader]


        public DateTime? ToDate { get; set; }
        [FromHeader]

        public long CreateBy { get; set; }
        [FromHeader]

        public int? PageNo { get; set; }
        [FromHeader]

        public int? NoOfItems { get; set; }
    }
}
