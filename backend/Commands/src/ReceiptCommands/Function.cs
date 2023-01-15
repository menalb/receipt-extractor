using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using ReceiptCommand.Model;
using System.Linq;
using Amazon.DynamoDBv2.DataModel;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ReceiptCommands
{
    public class Functions
    {

        private readonly IAmazonDynamoDB _dynamoDBClient;

        private string ReceiptItemsTableName = Environment.GetEnvironmentVariable("RECEIPT_ITEMS_TABLE_NAME");

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            _dynamoDBClient = new AmazonDynamoDBClient();
        }

        public Functions(IAmazonDynamoDB dynamoDBClinet)
        {
            _dynamoDBClient = dynamoDBClinet;
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The API Gateway response.</returns>
        public async Task<APIGatewayProxyResponse> Put(APIGatewayProxyRequest request, ILambdaContext context)
        {
            context.Logger.LogLine("Put Request\n");

            if (request.RequestContext.Authorizer == null ||
                string.IsNullOrEmpty(request.RequestContext.Authorizer.Claims["sub"]))
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.MethodNotAllowed,
                    Headers = new Dictionary<string, string>
                    {
                         { "Content-Type", "application/json" } ,
                         {"Access-Control-Allow-Origin", "*"},
                         {"Access-Control-Allow-Methods", "*"},
                    }
                };
            }

            var userId = request.RequestContext.Authorizer.Claims["sub"];
            var receiptId = request.PathParameters["receiptid"];

            var body = System.Text.Json.JsonSerializer.Deserialize<ReceiptDetails>(request.Body);

            await UpdateReceipt(receiptId, userId, body);

            var response = new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Headers = new Dictionary<string, string>
                {
                     { "Content-Type", "application/json" } ,
                     {"Access-Control-Allow-Origin", "*"},
                     {"Access-Control-Allow-Methods", "*"},
                }
            };

            return response;
        }
        private async Task UpdateReceipt(string receiptId, string userId, ReceiptDetails receipt)
        {
            var ctx = new DynamoDBContext(_dynamoDBClient);

            var shop = receipt.Shop == null ?
                       new UpdateShop() :
                       new UpdateShop
                       {
                           Name = receipt.Shop.Name,
                           Owner = receipt.Shop.Owner,
                           Address = receipt.Shop.Address,
                           City = receipt.Shop.City,
                           Phone = receipt.Shop.Phone,
                           Vat = receipt.Shop.Vat
                       };

            var items = receipt.Items == null ?
                        new List<UpdateReceiptItem>() :
                        receipt.Items.Select(item => new UpdateReceiptItem
                        {
                            Name = item.Name,
                            Price = item.Price,
                            VAT = item.VAT
                        });

            var updateReceipt = new UpdateReceiptDetails
            {
                Id = receiptId,
                UserId = userId,
                Day = receipt.Day,
                JobId = receipt.JobId,
                Total = receipt.Total,
                TotalVAT = receipt.TotalVAT,
                Shop = shop,
                Items = items.ToList(),
                Tags = receipt.Tags != null ? receipt.Tags.ToList() : new List<string>(),
                Notes = receipt.Notes
            };

            await ctx.SaveAsync(updateReceipt);
        }
    }
}
