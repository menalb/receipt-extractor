using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using ReceiptCommand.Model;
using ReceiptCommands.Handlers;
using Microsoft.Extensions.DependencyInjection;
using ReceiptCommands.HttpUtils;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ReceiptCommands;

public class Functions
{
    private readonly ServiceProvider _serviceProvider;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Functions()
    {
        _serviceProvider = ConfigureServices();
    }

    public Functions(ServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The API Gateway response.</returns>
    public async Task<APIGatewayProxyResponse> Put(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogLine($"{nameof(Put)} Request\n");

        if (!IsAuthorized(request))
        {
            return ApiGatewayResponse.MethodNotAllowed();
        }

        (bool isValid, ParsedPutRequest parsed) = ParsePutRequest(request);

        if (!isValid)
        {
            return ApiGatewayResponse.BadRequest();
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var updateCommandHandler = scope
                .ServiceProvider
                .GetRequiredService<ICommandHandler<UpdateReceiptCommand, ReceiptId>>();
            await updateCommandHandler.Handle(parsed.UserId, parsed.Command);
        }
        return ApiGatewayResponse.OK();
    }

    /// <summary>
    /// A Lambda function to respond to HTTP Get methods from API Gateway
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The API Gateway response.</returns>
    public async Task<APIGatewayProxyResponse> Post(APIGatewayProxyRequest request, ILambdaContext context)
    {
        context.Logger.LogLine($"{nameof(Post)} Request\n");

        if (!IsAuthorized(request))
        {
            return ApiGatewayResponse.MethodNotAllowed();
        }

        (bool isValid, ParsedPostRequest parsed) = ParsePostRequest(request);

        context.Logger.LogLine("is valid: " + isValid);

        if (!isValid)
        {
            return ApiGatewayResponse.BadRequest();
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var registerCommandHandler = scope
                .ServiceProvider
                .GetRequiredService<ICommandHandler<RegisterReceiptCommand, ReceiptId>>();

            var id = await registerCommandHandler.Handle(parsed.UserId, parsed.Command);

            context.Logger.LogLine("id: " + id);
        }

        return ApiGatewayResponse.OK();
    }

    private static bool IsAuthorized(APIGatewayProxyRequest request) =>
        request is not null &&
        request.RequestContext is not null &&
        request.RequestContext.Authorizer is not null &&
        !string.IsNullOrEmpty(request.RequestContext.Authorizer.Claims["sub"]);

    private static (bool isValid, ParsedPutRequest parsed) ParsePutRequest(APIGatewayProxyRequest request)
    {

        if (request.PathParameters is null || string.IsNullOrEmpty(request.Body))
        {
            return (false, null);
        }

        var userId = request.RequestContext.Authorizer.Claims["sub"];
        var receiptId = request.PathParameters["receiptid"];

        var receipt = System.Text.Json.JsonSerializer.Deserialize<ReceiptDetails>(request.Body);
        var isValid = !(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(receiptId) || receipt is null);
        return (
            isValid,
            isValid
            ? new ParsedPutRequest(userId, new UpdateReceiptCommand(receiptId, receipt))
            : null);
    }

    private static (bool isValid, ParsedPostRequest parsed) ParsePostRequest(APIGatewayProxyRequest request)
    {
        if (string.IsNullOrEmpty(request.Body))
        {
            return (false, null);
        }

        var userId = request.RequestContext.Authorizer.Claims["sub"];

        var receipt = System.Text.Json.JsonSerializer.Deserialize<ReceiptDetails>(request.Body);

        if (string.IsNullOrEmpty(receipt.Day))
        {
            return (false, null);
        }

        return (
            receipt is not null,
            true ? new ParsedPostRequest(userId, new RegisterReceiptCommand(receipt))
            : null);
    }

    record ParsedPostRequest(string UserId, RegisterReceiptCommand Command);
    record ParsedPutRequest(string UserId, UpdateReceiptCommand Command);

    private static ServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        
        serviceCollection.AddScoped<IAmazonDynamoDB, AmazonDynamoDBClient>();
        serviceCollection.AddScoped<ISaveReceiptsGateway, DynamoDBReceiptsGateway>();
        serviceCollection.AddScoped<IReceiptIdGenerator>(ctx => new ReceiptIdGenerator());

        serviceCollection.AddScoped<ICommandHandler<RegisterReceiptCommand, ReceiptId>, RegisterReceiptCommandHandler>();

        serviceCollection.AddScoped<ICommandHandler<UpdateReceiptCommand, ReceiptId>, UpdateReceiptCommandHandler>();

        return serviceCollection.BuildServiceProvider();
    }
}
