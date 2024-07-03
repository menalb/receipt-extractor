using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.S3;
using Amazon.S3.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReceiptApp.Images.Functions.Gateways;

public class ReceiptImageGateway
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonDynamoDB _dynamoDBClient;
    private readonly string _bucketName;


    public ReceiptImageGateway(IAmazonS3 s3Client, string bucketName, IAmazonDynamoDB dynamoDBClient)
    {
        _s3Client = s3Client;
        _bucketName = bucketName;
        _dynamoDBClient = dynamoDBClient;
    }

    public async Task<GetObjectResponse> GetReceiptImagePath(string userId, string receiptId)
    {
        var job = await GetJobId(userId,receiptId);
        var imagePath = await GetImagePath(job,userId);
        
        using var s3ObjectResponse = await _s3Client.GetObjectAsync(_bucketName, imagePath);

        return s3ObjectResponse;
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

        var raw = await _dynamoDBClient.GetItemAsync(get);

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

        var raw = await _dynamoDBClient.GetItemAsync(get);

        if (raw.Item.Count == 0)
        {
            return null;
        }

        return raw.Item["file_path"].S;
    }
}

record ReceiptImageInfo(string Path, string ThumbnailPath);
