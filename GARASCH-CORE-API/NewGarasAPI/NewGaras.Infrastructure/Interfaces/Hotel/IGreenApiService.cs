using NewGaras.Infrastructure.Models.Hotel;

namespace NewGaras.Infrastructure.Interfaces.Hotel
{
    public interface IGreenApiService
    {
        Task<bool> SendMessage(string mobile , string ContentMessage);
        Task<bool> SendPdfbyUrl(string mobile , string urlFile , string fileName , string caption);
        Task<bool> UploadFormDataAsync(FormOfFileMessage formData , long UserId);

    }
}
