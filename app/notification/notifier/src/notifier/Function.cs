using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;

using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;

using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

using MongoDB.Bson;
using MongoDB.Driver;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace notifier;

public class Function
{
    private readonly IAmazonApiGatewayManagementApi _apiGWClient;
    private readonly IAmazonSimpleSystemsManagement _ssmClient;

    // Settings come from AWS Systems Manager Parameter Store, never from a committed file:
    // MONGO_CONNECTION_STRING_PARAM only names the SSM parameter that holds the real
    // connection string (a SecureString), it is not the secret itself.
    private readonly string MongoConnectionStringParameterName = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING_PARAM") ?? throw new ArgumentNullException("MONGO_CONNECTION_STRING_PARAM");
    private readonly string MongoDatabaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME") ?? throw new ArgumentNullException("MONGO_DATABASE_NAME");
    private readonly string MongoCollectionName = Environment.GetEnvironmentVariable("MONGO_COLLECTION_NAME") ?? throw new ArgumentNullException("MONGO_COLLECTION_NAME");

    AmazonApiGatewayManagementApiConfig configuration = new AmazonApiGatewayManagementApiConfig()
    {
        AuthenticationRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? throw new ArgumentNullException("AWS_REGION"),
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION") ?? throw new ArgumentNullException("AWS_REGION")),
        ServiceURL = Environment.GetEnvironmentVariable("NOTIFICATION_API_URL") ?? throw new ArgumentNullException("NOTIFICATION_API_URL")
    };

    // Cached across warm invocations of the same Lambda execution environment, so we only hit
    // SSM / open a Mongo connection once per cold start rather than on every invocation.
    private static Lazy<Task<IMongoCollection<BsonDocument>>>? _connectionsCollection;

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        _apiGWClient = new AmazonApiGatewayManagementApiClient(configuration);
        _ssmClient = new AmazonSimpleSystemsManagementClient();
    }

    public Function(IAmazonApiGatewayManagementApi apiGWClient, IAmazonSimpleSystemsManagement ssmClient)
    {
        _apiGWClient = apiGWClient;
        _ssmClient = ssmClient;
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

    private Task<IMongoCollection<BsonDocument>> GetConnectionsCollectionAsync()
    {
        _connectionsCollection ??= new Lazy<Task<IMongoCollection<BsonDocument>>>(async () =>
        {
            var parameter = await _ssmClient.GetParameterAsync(new GetParameterRequest
            {
                Name = MongoConnectionStringParameterName,
                WithDecryption = true
            });

            var client = new MongoClient(parameter.Parameter.Value);
            return client.GetDatabase(MongoDatabaseName).GetCollection<BsonDocument>(MongoCollectionName);
        });

        return _connectionsCollection.Value;
    }

    private async Task<IEnumerable<string>> GetConnectionToNotify(string userId)
    {
        // connectionId is stored as the Mongo document's _id (see onconnect/ondisconnect).
        var collection = await GetConnectionsCollectionAsync();
        var filter = Builders<BsonDocument>.Filter.Eq("userId", userId);
        var documents = await collection.Find(filter).ToListAsync();
        return documents.Select(d => d["_id"].AsString);
    }

    public class NotificationMessage
    {
        public string userId { get; set; } = "";
        public string receiptId { get; set; } = "";
    }
}
