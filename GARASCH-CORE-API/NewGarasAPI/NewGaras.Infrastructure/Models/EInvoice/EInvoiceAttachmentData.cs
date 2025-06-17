namespace NewGaras.Infrastructure.Models.EInvoice
{
    public class EInvoiceAttachmentData
    {
        public long? Id { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public IFormFile File { get; set; }
        public string FilePath { get; set; }
        public string Category { get; set; }
        public bool Active { get; set; }

        public string AttachmentPath { get; set; }

        public string Type { get; set; }
    }
}