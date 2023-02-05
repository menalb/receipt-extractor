using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;
using Moq;
using Amazon.S3;
using Amazon.S3.Model;

namespace ReceiptApp.Images.Functions.Tests
{
    public class FunctionTest
    {
        public FunctionTest()
        {
        }

        [Fact]
        public void Whe_Authorized_Is_Missing_Fails()
        {
            var functions = new Functions();
            var request = new APIGatewayProxyRequest
            {
                RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
                {

                }
            };
            var context = new TestLambdaContext();

            var response = functions.GetForUpload(request, context);

            Assert.Equal((int)System.Net.HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        [Fact]
        public void Whe_Caller_NotAuthorized_Fails()
        {
            var authorizer = new APIGatewayCustomAuthorizerContext
            {
                Claims = new System.Collections.Generic.Dictionary<string, string> { { "sub", "" } }
            };
            var functions = new Functions();
            var request = new APIGatewayProxyRequest
            {
                RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
                {
                    Authorizer = authorizer
                }
            };
            var context = new TestLambdaContext();

            var response = functions.GetForUpload(request, context);

            Assert.Equal((int)System.Net.HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }


        [Fact]
        public void Whe_Is_Authorized_Returns_OK()
        {
            var expectedURL = "url";
            var s3Client = new Mock<IAmazonS3>();

            s3Client
                .Setup(c => c.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
                .Returns(expectedURL);

            var authorizer = new APIGatewayCustomAuthorizerContext
            {
                Claims = new System.Collections.Generic.Dictionary<string, string> { { "sub", "123" } }
            };

            var functions = new Functions(s3Client.Object);

            var request = new APIGatewayProxyRequest
            {
                RequestContext = new APIGatewayProxyRequest.ProxyRequestContext
                {
                    Authorizer = authorizer
                }
            };
            var context = new TestLambdaContext();

            var response = functions.GetForUpload(request, context);

            Assert.Equal((int)System.Net.HttpStatusCode.OK, response.StatusCode);
        }
    }

}
