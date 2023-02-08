using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;

namespace ReceiptApp.Images.Functions;

public class S3ImagesGateway : IImagesGateway
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonDynamoDB _dynamodbClient;
    private readonly string _bucketname;
    private const string ContentType = "image/jpeg";

    public S3ImagesGateway(IAmazonS3 s3Client, string bucketname, IAmazonDynamoDB dynamodbClient)
    {
        _s3Client = s3Client;
        _bucketname = bucketname;
        _dynamodbClient = dynamodbClient;
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

    public async Task<GetImageURLResponse> GetImageURL(string userId, string receiptId)
    {
        var key = $"{userId}/{receiptId}.jpeg";

        if (await ExistsFile(_bucketname, key))
        {
            return GetPresignedUrl(key);
        }

        var jobId = await GetJobId(userId, receiptId);
        
        if (jobId != null)
        {
            var path = await GetImagePath(userId, jobId);
            return GetPresignedUrl(path);
        }

        throw new Exception($"Unable to generate pre-signed URL for {receiptId}");
    }

    private GetImageURLResponse GetPresignedUrl(string key)
    {
        var s3Params = new GetPreSignedUrlRequest
        {
            BucketName = _bucketname,
            Key = key,
            Expires = DateTime.Now.AddMinutes(15)
        };

        var url = _s3Client.GetPreSignedURL(s3Params);
        return new(key, url);
    }

    private async Task<bool> ExistsFile(string bucketName, string key)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(bucketName, key);
            return true;
        }
        catch (Exception ex)
        {
            
        }
        return false;
    }

    private async Task<string> GetJobId(string userId, string receiptId)
    {
        var get = new GetItemRequest
        {
            TableName = "receipt-analyzer-raw",
            Key = new Dictionary<string, AttributeValue>
            {
                {"user_id", new AttributeValue { S =  userId }},
                {"receipt_id", new AttributeValue { S =  receiptId }}
            },
            ProjectionExpression = "job_id",
        };

        var raw = await _dynamodbClient.GetItemAsync(get);

        if (raw.Item.Count == 0)
        {
            return null;
        }

        return raw.Item["job_id"].S;
    }

    private async Task<string> GetImagePath(string userId, string jobId)
    {
        var get = new GetItemRequest
        {
            TableName = "receipts-analyzer-jobs",
            Key = new Dictionary<string, AttributeValue>
            {
                {"user_id", new AttributeValue { S =  userId }},
                {"job_id", new AttributeValue { S =  jobId }}
            },
            ProjectionExpression = "file_path",
        };

        var raw = await _dynamodbClient.GetItemAsync(get);

        if (raw.Item.Count == 0)
        {
            return null;
        }

        return raw.Item["file_path"].S;
    }
}
