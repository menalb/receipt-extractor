using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using ReceiptCommand.Model;
using ReceiptCommands.Handlers;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace ReceiptCommands;

public class Functions
{
    private readonly ICommandHandler<UpdateReceiptCommand, string> _updateCommadHandler;
    private readonly ICommandHandler<RegisterReceiptCommand, string> _registerCommadHandler;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Functions()
    {
        _updateCommadHandler = new UpdateReceiptCommandHandler(new AmazonDynamoDBClient());
        _registerCommadHandler = new RegisterReceiptCommandHandler(new AmazonDynamoDBClient());
    }

    public Functions(
        ICommandHandler<UpdateReceiptCommand, string> updateCommadHandler,
        ICommandHandler<RegisterReceiptCommand, string> registerCommadHandler
        )
    {
        _updateCommadHandler = updateCommadHandler;
        _registerCommadHandler = registerCommadHandler;
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
            return MethodNotAllowed;
        }

        (bool isValid, ParsedPutRequest parsed) = ParsePutRequest(request);

        if (!isValid)
        {
            return BadRequest;
        }

        await _updateCommadHandler.Handle(parsed.userId, parsed.command);

        return OK;
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
            return MethodNotAllowed;
        }

        (bool isValid, ParsedPostRequest parsed) = ParsePostRequest(request);

        context.Logger.LogLine("is valid: " + isValid);

        if (!isValid)
        {
            return BadRequest;
        }
        
        var id = await _registerCommadHandler.Handle(parsed.userId, parsed.command);

        context.Logger.LogLine("id: " + id);

        return OK;
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

    record ParsedPostRequest(string userId, RegisterReceiptCommand command);
    record ParsedPutRequest(string userId, UpdateReceiptCommand command);
    public APIGatewayProxyResponse OK = BuildResponse(HttpStatusCode.OK);
    public APIGatewayProxyResponse MethodNotAllowed = BuildResponse(HttpStatusCode.MethodNotAllowed);
    public APIGatewayProxyResponse BadRequest = BuildResponse(HttpStatusCode.BadRequest);

    public static APIGatewayProxyResponse BuildResponse(HttpStatusCode statusCode) => new()
    {
        StatusCode = (int)statusCode,
        Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" } ,
            {"Access-Control-Allow-Origin", "*"},
            {"Access-Control-Allow-Methods", "*"},
        }
    };
}
