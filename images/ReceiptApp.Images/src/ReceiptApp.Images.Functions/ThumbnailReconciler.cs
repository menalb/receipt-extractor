using ReceiptApp.Images.Functions.Gateways;
using System.Threading.Tasks;

namespace ReceiptApp.Images.Functions
{
    public class ThumbnailReconciler
    {
        private readonly ReceiptImageGateway _gateway;
        private readonly ThumbnailGenerator _thumbnailGenerator;
        private readonly string _thumbnailBucketName;

        public ThumbnailReconciler(ReceiptImageGateway gateway, ThumbnailGenerator thumbnailGenerator, string thumbnailBucketName)
        {
            _gateway = gateway;
            _thumbnailGenerator = thumbnailGenerator;
            _thumbnailBucketName = thumbnailBucketName;
        }

        public async Task Reconcile(string userId,string receiptId)
        {
            using var obj = await _gateway.GetReceiptImagePath(userId, receiptId);

            await _thumbnailGenerator.Generate(obj, _thumbnailBucketName);

            var s3key = obj.Key;
            var imageInfo = new ReceiptImageInfo(s3key, ThumbnailGenerator.GetThumbnailPath(s3key));
        }
    }
}
