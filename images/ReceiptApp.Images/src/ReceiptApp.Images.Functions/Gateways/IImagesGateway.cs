using System.Threading.Tasks;

namespace ReceiptApp.Images.Functions;

public interface IImagePresignedURLGateway
{
    string GetUploadURL(string imageName);
    Task<GetImageURLResponse> GetImageURL(string userId, string receiptId);
}

public record GetImageURLResponse(string Key, string URL);