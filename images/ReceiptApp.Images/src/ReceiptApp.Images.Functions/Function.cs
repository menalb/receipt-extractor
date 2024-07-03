using System;
using System.Collections.Generic;
using System.Net;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ReceiptApp.Images.Functions
{
    public class Functions
    {

        private readonly string BucketName = Environment.GetEnvironmentVariable("BucketName");
        private readonly string ThumbnailsBucketName = Environment.GetEnvironmentVariable("ThumbnailsBucketName");

        private readonly IImagePresignedURLGateway _imageGateway;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            var s3Client = new AmazonS3Client();
            var dynamoDbClient = new AmazonDynamoDBClient();
            _imageGateway = new S3ImagePresignedURLGateway(s3Client, BucketName, ThumbnailsBucketName, dynamoDbClient);
        }

        public Functions(IAmazonS3 s3Client, IAmazonDynamoDB dynamoDbClient)
        {
            _imageGateway = new S3ImagePresignedURLGateway(s3Client, BucketName, ThumbnailsBucketName, dynamoDbClient);
        }

        /// <summary>
        /// Generates PreSigned URL for Image Upload
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse GetForUpload(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine($"{nameof(GetForUpload)} Request\n");

            if (request.RequestContext.Authorizer == null
                ||
                string.IsNullOrEmpty(request.RequestContext.Authorizer.Claims["sub"]))
            {
                return MethodNotAllowed;
            }

            var prefix = request.RequestContext.Authorizer.Claims["sub"];

            var key = $"{prefix}/{Guid.NewGuid()}.jpeg";

            context.Logger.LogLine("key: " + key);

            var uploadURL = _imageGateway.GetUploadURL(key);

            var message = new ApiResponse(key, uploadURL);

            return OK(message);
        }

        /// <summary>
        /// Get PreSignedURL to display image
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task<APIGatewayProxyResponse> GetImage(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine($"{nameof(GetImage)} Request\n");

            if (request.RequestContext.Authorizer == null
                ||
                string.IsNullOrEmpty(request.RequestContext.Authorizer.Claims["sub"]))
            {
                return MethodNotAllowed;
            }

            var userId = request.RequestContext.Authorizer.Claims["sub"];
            var receiptId = request.PathParameters["receiptid"];

            try
            {
                var response = await _imageGateway.GetImageURL(userId, receiptId);

                if(response is null)
                {
                    context.Logger.LogLine($"File for receipt {receiptId} not found");
                    return NotFound;
                }

                context.Logger.LogLine("key: " + response.URL);

                var message = new ApiResponse(response.Key, response.URL);

                return OK(message);
            }
            catch (Exception e)
            {
                context.Logger.LogError(e.Message);
                return ServerError;
            }
        }

        private static APIGatewayProxyResponse OK(ApiResponse message) =>
            new()
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = System.Text.Json.JsonSerializer.Serialize(message),
                Headers = new Dictionary<string, string>
                 {
                     { "Content-Type", "application/json" },
                     {"Access-Control-Allow-Origin", "*"},
                     {"Access-Control-Allow-Methods", "*"},
                 }
            };

        private static APIGatewayProxyResponse MethodNotAllowed =>
            new()
            {
                StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    {"Access-Control-Allow-Origin", "*"},
                    {"Access-Control-Allow-Methods", "*"},
                }
            };

        private static APIGatewayProxyResponse NotFound =>
            new()
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    {"Access-Control-Allow-Origin", "*"},
                    {"Access-Control-Allow-Methods", "*"},
                }
            };

        private static APIGatewayProxyResponse ServerError =>
           new()
           {
               StatusCode = (int)HttpStatusCode.InternalServerError,
               Headers = new Dictionary<string, string>
               {
                    { "Content-Type", "application/json" },
                    {"Access-Control-Allow-Origin", "*"},
                    {"Access-Control-Allow-Methods", "*"},
               }
           };
    }

    record ApiResponse(string Key, string UploadURL);
}
