using Amazon.Lambda.APIGatewayEvents;
using System.Collections.Generic;
using System.Net;

namespace ReceiptCommands.HttpUtils
{
    internal class ApiGatewayResponse
    {
        public static APIGatewayProxyResponse OK() => BuildResponse(HttpStatusCode.OK);
        public static APIGatewayProxyResponse MethodNotAllowed() => BuildResponse(HttpStatusCode.MethodNotAllowed);
        public static APIGatewayProxyResponse BadRequest() => BuildResponse(HttpStatusCode.BadRequest);
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
}
