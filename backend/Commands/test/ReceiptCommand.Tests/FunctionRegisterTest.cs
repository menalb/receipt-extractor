using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using ReceiptCommands;
using System.Net;
using System.Collections.Generic;
using ReceiptCommand.Model;
using Microsoft.Extensions.DependencyInjection;
using ReceiptCommands.Handlers;

namespace ReceiptCommand.Tests;

public class FunctionPostTest
{
    private readonly Functions _functions;
    private readonly FakeUpdateCommandHandler _updateCommandHandler;
    private readonly FakeRegisterCommandHandler _registerCommandHandler;

    public FunctionPostTest()
    {
        _updateCommandHandler = new FakeUpdateCommandHandler();
        _registerCommandHandler = new FakeRegisterCommandHandler();

        var serviceCollection = new ServiceCollection();               
        serviceCollection.AddScoped<ICommandHandler<RegisterReceiptCommand, ReceiptId>>(c=>_registerCommandHandler);
        serviceCollection.AddScoped<ICommandHandler<UpdateReceiptCommand, ReceiptId>>(c=>_updateCommandHandler);
        _functions = new Functions(serviceCollection.BuildServiceProvider());
    }

    [Fact]
    public async Task When_The_User_In_Not_Authorized_It_Returns_MethodNotAllowed()
    {
        var request = new APIGatewayProxyRequest();
        var context = new TestLambdaContext();

        var response = await _functions.Post(request, context);

        Assert.Equal((int)HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task When_The_Day_Is_Missing_It_Returns_BadRequest()
    {
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "user_sub" } }
                }
            },
            Body = System.Text.Json.JsonSerializer.Serialize(new ReceiptDetails())
        };
        var context = new TestLambdaContext();

        var response = await _functions.Post(request, context);

        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task When_The_User_In_Authorized_And_The_Receipt_Data_Are_Valid_It_Returns_Performs_The_Updates()
    {            
        var request = new APIGatewayProxyRequest
        {
            RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
            {
                Authorizer = new APIGatewayCustomAuthorizerContext
                {
                    Claims = new Dictionary<string, string> { { "sub", "user_sub" } }
                }
            },
            Body = System.Text.Json.JsonSerializer.Serialize(new ReceiptDetails { Day = "2023-01-01"})
        };
        var context = new TestLambdaContext();

        var response = await _functions.Post(request, context);

        Assert.NotEmpty(_registerCommandHandler.RegisteredReceiptIds);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    }       
}
