
namespace NewGaras.Domain.Services.Purchasing
{
    internal class PurchasePOShipmentDocument
    {
        public PurchasePOShipmentDocument()
        {
        }

        public long PurchasePOShipmentID { get; set; }
        public string AttachmentPath { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public bool Active { get; set; }
        public object FileName { get; set; }
        public object FileExtenssion { get; set; }
        public DateTime ReceivedIn { get; set; }
        public decimal? Amount { get; set; }
        public int? CurrencyID { get; set; }
    }
}