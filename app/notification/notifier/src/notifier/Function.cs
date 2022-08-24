using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;

using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace notifier;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoDBClient;
    private readonly IAmazonApiGatewayManagementApi _apiGWClient;
    private string NotificationTableName = Environment.GetEnvironmentVariable("NOTIFICATION_TABLE_NAME") ?? throw new ArgumentNullException("NOTIFICATION_TABLE_NAME");
    AmazonApiGatewayManagementApiConfig configuration = new AmazonApiGatewayManagementApiConfig()
    {
        AuthenticationRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? throw new ArgumentNullException("AWS_REGION"),
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? throw new ArgumentNullException("AWS_REGION")),
        ServiceURL = Environment.GetEnvironmentVariable("NOTIFICATION_API_URL") ?? throw new ArgumentNullException("NOTIFICATION_API_URL")
    };

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        _dynamoDBClient = new AmazonDynamoDBClient();
        _apiGWClient = new AmazonApiGatewayManagementApiClient(configuration);
    }

    public Function(IAmazonDynamoDB dynamoDBClient)
    {
        _dynamoDBClient = dynamoDBClient;
        _apiGWClient = new AmazonApiGatewayManagementApiClient(configuration);
    }


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SNS event object and can be used 
    /// to respond to SNS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(SNSEvent evnt, ILambdaContext context)
    {
        foreach (var record in evnt.Records)
        {
            await ProcessRecordAsync(record, context);
        }
    }

    private async Task ProcessRecordAsync(SNSEvent.SNSRecord record, ILambdaContext context)
    {
        context.Logger.LogInformation($"request {record}");
        context.Logger.LogInformation($"Processed record {record.Sns.Message}");

        var message = System.Text.Json.JsonSerializer.Deserialize<NotificationMessage>(record.Sns.Message);        

        if (message is not null && !string.IsNullOrEmpty(message.userId))
        {

            var connections = await GetConnectionToNotify(message.userId);

            if (connections.Any())
            {
                foreach (var connectionId in connections)
                {
                    try
                    {
                        PostToConnectionRequest awsRequest = new PostToConnectionRequest
                        {
                            ConnectionId = connectionId,
                            Data = BuildNotificationMessage(message)
                        };

                        PostToConnectionResponse response = await _apiGWClient.PostToConnectionAsync(awsRequest);

                        context.Logger.LogInformation($"connection: {connectionId} : {response.HttpStatusCode.ToString()}");
                    }
                    catch (Amazon.ApiGatewayManagementApi.Model.GoneException e)
                    {
                        context.Logger.LogWarning($"connection: {connectionId} is gone ${e.StatusCode}");
                    }
                    catch (Exception e)
                    {
                        context.Logger.LogError(e.Message);
                    }
                }
            }
        }
    }

    private MemoryStream BuildNotificationMessage(NotificationMessage message)
    {
        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(message)));
    }

    private async Task<IEnumerable<string>> GetConnectionToNotify(string userId)
    {
        var queryRequest = new Amazon.DynamoDBv2.Model.QueryRequest
        {
            TableName = NotificationTableName,
            IndexName = "UserIdIndex",
            KeyConditionExpression = "#userId = :v_userId",
            ExpressionAttributeNames = new Dictionary<string, string>{
                    {"#userId","userId"}
                },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                {":v_userId", new AttributeValue { S =  userId }}
            },
            ScanIndexForward = true

        };

        var result = await _dynamoDBClient.QueryAsync(queryRequest);
        if (result != null)
        {
            return result.Items.Select(i => i["connectionId"].S);
        }
        return Enumerable.Empty<string>();
    }

    public class NotificationMessage
    {
        public string userId { get; set; } = "";
        public string receiptId { get; set; } = "";
    }
}