using System;
using Amazon.S3;
using Amazon.S3.Model;

namespace ReceiptApp.Images.Functions;

public class S3ImagesGateway : IImagesGateway
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketname;
    private const string ContentType = "image/jpeg";
    public S3ImagesGateway(IAmazonS3 s3Client, string bucketname)
    {
        _s3Client = s3Client;
        _bucketname = bucketname;
    }

    public string GetUploadURL(string imageName)
    {
        var s3Params = new GetPreSignedUrlRequest
        {
            BucketName = _bucketname,
            Key = imageName,
            Expires = DateTime.Now.AddMinutes(5),
            ContentType = ContentType,
            Verb = HttpVerb.PUT,
        };

        return _s3Client.GetPreSignedURL(s3Params);
    }

    public string GetImageURL(string imageName)
    {
        var s3Params = new GetPreSignedUrlRequest
        {
            BucketName = _bucketname,
            Key = imageName,
            Expires = DateTime.Now.AddMinutes(15)
        };

        return _s3Client.GetPreSignedURL(s3Params);
    }
}
