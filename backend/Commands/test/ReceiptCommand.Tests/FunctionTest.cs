using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using ReceiptCommands;
using System.Net;
using System.Collections.Generic;
using ReceiptCommands.Handlers;
using ReceiptCommand.Model;
using System;

namespace ReceiptCommand.Tests
{
    public class FunctionPutTest
    {
        private readonly Functions _functions;
        private readonly FakeUpdateCommandHandler _updateCommandHandler;
        private readonly FakeRegisterCommandHandler _registerCommandHandler;

        public FunctionPutTest()
        {
            _updateCommandHandler = new FakeUpdateCommandHandler();
            _registerCommandHandler = new FakeRegisterCommandHandler();
            _functions = new Functions(_updateCommandHandler, _registerCommandHandler);
        }

        [Fact]
        public async Task When_The_User_In_Not_Authorized_It_Returns_MethodNotAllowed()
        {
            var request = new APIGatewayProxyRequest();
            var context = new TestLambdaContext();

            var response = await _functions.Put(request, context);

            Assert.Equal((int)HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public async Task When_The_ReceiptId_Is_Missing_It_Returns_BadRequest()
        {
            var request = new APIGatewayProxyRequest
            {
                RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
                {
                    Authorizer = new APIGatewayCustomAuthorizerContext
                    {
                        Claims = new Dictionary<string, string> { { "sub", "user_sub" } }
                    }
                }
            };
            var context = new TestLambdaContext();

            var response = await _functions.Put(request, context);

            Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task When_The_User_In_Authorized_And_The_Receipt_Data_Are_Valid_It_Returns_Performs_The_Updates()
        {
            var receiptId = "123";
            var request = new APIGatewayProxyRequest
            {
                RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
                {
                    Authorizer = new APIGatewayCustomAuthorizerContext
                    {
                        Claims = new Dictionary<string, string> { { "sub", "user_sub" } }
                    }
                },
                PathParameters = new Dictionary<string, string> { { "receiptid", receiptId } },
                Body = System.Text.Json.JsonSerializer.Serialize(new ReceiptDetails())
            };
            var context = new TestLambdaContext();

            var response = await _functions.Put(request, context);

            Assert.Contains(receiptId, _updateCommandHandler.UpdatedReceiptIds);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class FakeUpdateCommandHandler : ICommandHandler<UpdateReceiptCommand>
    {
        public List<string> UpdatedReceiptIds { get; } = new List<string>();

        public Task Handle(string userId, UpdateReceiptCommand command)
        {
            UpdatedReceiptIds.Add(command.ReceiptId);
            return Task.CompletedTask;
        }
    }

    public class FakeRegisterCommandHandler : ICommandHandler<RegisterReceiptCommand>
    {
        public List<string> RegisteredReceiptIds { get; } = new List<string>();

        public Task Handle(string userId, RegisterReceiptCommand command)
        {
            RegisteredReceiptIds.Add(Guid.NewGuid().ToString());
            return Task.CompletedTask;
        }
    }

    public class FunctionPostTest
    {
        private readonly Functions _functions;
        private readonly FakeUpdateCommandHandler _updateCommandHandler;
        private readonly FakeRegisterCommandHandler _registerCommandHandler;

        public FunctionPostTest()
        {
            _updateCommandHandler = new FakeUpdateCommandHandler();
            _registerCommandHandler = new FakeRegisterCommandHandler();
            _functions = new Functions(_updateCommandHandler, _registerCommandHandler);
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
                Body = System.Text.Json.JsonSerializer.Serialize(new ReceiptDetails())
            };
            var context = new TestLambdaContext();

            var response = await _functions.Post(request, context);

            Assert.NotEmpty(_registerCommandHandler.RegisteredReceiptIds);
            Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        }
    }

}
