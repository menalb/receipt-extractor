using System;
using System.Collections.Generic;
using System.Net;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.S3;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ReceiptApp.Images.Functions
{
    public class Functions
    {

        private readonly string BucketName = Environment.GetEnvironmentVariable("BucketName");

        private readonly IImagesGateway _imageGateway;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions() 
        {
            var s3Client = new AmazonS3Client();
            _imageGateway = new S3ImagesGateway(s3Client, BucketName);
        }

        public Functions(IAmazonS3 s3Client)
        {
            _imageGateway = new S3ImagesGateway(s3Client, BucketName);
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
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
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public APIGatewayProxyResponse GetImage(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine($"{nameof(GetImage)} Request\n");

            if (request.RequestContext.Authorizer == null
                ||
                string.IsNullOrEmpty(request.RequestContext.Authorizer.Claims["sub"]))
            {
                return MethodNotAllowed;
            }

            var prefix = request.RequestContext.Authorizer.Claims["sub"];
            var receiptId = request.PathParameters["receiptid"];

            var key = $"{prefix}/{receiptId}.jpeg";

            context.Logger.LogLine("key: " + key);

            var uploadURL = _imageGateway.GetImageURL(key);

            context.Logger.LogLine("key: " + uploadURL);

            var message = new ApiResponse(key, uploadURL);

            return OK(message);
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
    }

    record ApiResponse(string Key, string UploadURL);
}
