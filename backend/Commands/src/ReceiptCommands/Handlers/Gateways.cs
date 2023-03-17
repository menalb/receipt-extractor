using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using ReceiptCommand.Model;
using System.Threading.Tasks;

namespace ReceiptCommands.Handlers;

public interface ISaveReceiptsGateway
{
    Task SaveAsync(SaveReceiptDetails ReceiptDetails);
}

public class DynamoDBReceiptsGateway : ISaveReceiptsGateway
{
    private readonly IAmazonDynamoDB _dynamoDBClient;

    public DynamoDBReceiptsGateway(IAmazonDynamoDB dynamoDBClient)
    {
        _dynamoDBClient = dynamoDBClient;
    }

    public async Task SaveAsync(SaveReceiptDetails updateReceipt)
    {
        using var ctx = new DynamoDBContext(_dynamoDBClient);
        await ctx.SaveAsync(updateReceipt);
    }
}