namespace ReceiptApp.Images.Functions;

public interface IImagesGateway
{
    string GetUploadURL(string imageName);
    string GetImageURL(string imageName);
}
