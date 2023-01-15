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
    private readonly ICommandHandler<UpdateReceiptCommand> _updateCommadHandler;

    /// <summary>
    /// Default constructor that Lambda will invoke.
    /// </summary>
    public Functions()
    {
        _updateCommadHandler = new UpdateReceiptCommandHandler(new AmazonDynamoDBClient());
    }

    public Functions(ICommandHandler<UpdateReceiptCommand> updateCommadHandler)
    {
        _updateCommadHandler = updateCommadHandler;
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

        (bool isValid, ParsedRequest parsed) = ParseRequest(request);

        if (!isValid)
        {
            return BadRequest;
        }

        await _updateCommadHandler.Handle(parsed.userId, parsed.command);

        return OK;
    }

    private static bool IsAuthorized(APIGatewayProxyRequest request) =>
        request is not null &&
        request.RequestContext is not null &&
        request.RequestContext.Authorizer is not null &&
        !string.IsNullOrEmpty(request.RequestContext.Authorizer.Claims["sub"]);

    private static (bool isValid, ParsedRequest parsed) ParseRequest(APIGatewayProxyRequest request)
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
            ? new ParsedRequest(userId, new UpdateReceiptCommand(receiptId, receipt)) 
            : null);
    }

    private static bool IsPutRequestValid(APIGatewayProxyRequest request)
    {
        if (request.PathParameters is null)
        {
            return false;
        }
        var userId = request.RequestContext.Authorizer.Claims["sub"];
        var receiptId = request.PathParameters["receiptid"];

        return !(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(receiptId));
    }

    record ParsedRequest(string userId, UpdateReceiptCommand command);
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
