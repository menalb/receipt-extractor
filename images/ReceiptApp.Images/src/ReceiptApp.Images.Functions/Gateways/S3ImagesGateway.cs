using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;

namespace ReceiptApp.Images.Functions;

public class S3ImagePresignedURLGateway : IImagePresignedURLGateway
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonDynamoDB _dynamoDBClient;
    private readonly string _bucketName;
    private readonly string _thumbnailBucketName;
    private const string ContentType = "image/jpeg";

    public S3ImagePresignedURLGateway(IAmazonS3 s3Client, string bucketName, string thumbnailBucketName, IAmazonDynamoDB dynamoDBClient)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
        _thumbnailBucketName = thumbnailBucketName;
        _dynamoDBClient = dynamoDBClient;
    }

    public string GetUploadURL(string imageName)
    {
        var s3Params = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = imageName,
            Expires = DateTime.Now.AddMinutes(5),
            ContentType = ContentType,
            Verb = HttpVerb.PUT,
        };

        return _s3Client.GetPreSignedURL(s3Params);
    }

    public async Task<GetImageURLResponse> GetImageURL(string userId, string receiptId)
    {
        var key = await GetThumbnailPath(userId, receiptId);

        if(string.IsNullOrEmpty(key))
        {
            return null;
        }
        return GetPreSignedUrl(_thumbnailBucketName, key);
    }   

    private GetImageURLResponse GetPreSignedUrl(string bucketName, string key)
    {
        var s3Params = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Expires = DateTime.Now.AddMinutes(15)
        };

        var url = _s3Client.GetPreSignedURL(s3Params);
        return new(key, url);
    }

    private async Task<string> GetThumbnailPath(string userId, string receiptId)
    {
        var get = new GetItemRequest
        {
            TableName = "receipts-items",
            Key = new Dictionary<string, AttributeValue>
            {
                {"user_id", new AttributeValue { S =  userId }},
                {"receipt_id", new AttributeValue { S =  receiptId }}
            },
            ProjectionExpression = "image",
        };

        var raw = await _dynamoDBClient.GetItemAsync(get);

        if (raw.Item.Count == 0 || !raw.Item.ContainsKey("image"))
        {
            return null;
        }

        var image = raw.Item["image"].M;
        return image.ContainsKey("thumbnail") ? image["thumbnail"].S : string.Empty;
    }
}
