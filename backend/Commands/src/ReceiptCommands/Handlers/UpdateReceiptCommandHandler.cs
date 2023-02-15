using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using ReceiptCommand.Model;
using System.Linq;
using Amazon.DynamoDBv2.DataModel;

namespace ReceiptCommands.Handlers;

public class UpdateReceiptCommand
{
    public UpdateReceiptCommand(string receiptId, ReceiptDetails receiptDetails)
    {
        ReceiptId = receiptId;
        ReceiptDetails = receiptDetails;
    }

    public string ReceiptId { get; }
    public ReceiptDetails ReceiptDetails { get; }
}
public class UpdateReceiptCommandHandler : ICommandHandler<UpdateReceiptCommand, string>
{
    private readonly IAmazonDynamoDB _dynamoDBClient;

    public UpdateReceiptCommandHandler(IAmazonDynamoDB db)
    {
        _dynamoDBClient = db;
    }

    public async Task<string> Handle(string userId, UpdateReceiptCommand command)
    {
        string receiptId = command.ReceiptId;
        ReceiptDetails receipt = command.ReceiptDetails;

        var ctx = new DynamoDBContext(_dynamoDBClient);

        var shop = receipt.Shop == null ?
                   new UpdateShop() :
                   new UpdateShop
                   {
                       Name = receipt.Shop.Name,
                       Owner = receipt.Shop.Owner,
                       Address = receipt.Shop.Address,
                       City = receipt.Shop.City,
                       Phone = receipt.Shop.Phone,
                       Vat = receipt.Shop.Vat
                   };

        var items = receipt.Items == null ?
                    new List<UpdateReceiptItem>() :
                    receipt.Items.Select(item => new UpdateReceiptItem
                    {
                        Name = item.Name,
                        Price = item.Price,
                        VAT = item.VAT
                    });

        var updateReceipt = new UpdateReceiptDetails
        {
            Id = receiptId,
            UserId = userId,
            Day = receipt.Day,
            JobId = receipt.JobId,
            Total = receipt.Total,
            TotalVAT = receipt.TotalVAT,
            Shop = shop,
            Items = items.ToList(),
            Tags = receipt.Tags != null ? receipt.Tags.ToList() : new List<string>(),
            Notes = receipt.Notes
        };

        await ctx.SaveAsync(updateReceipt);

        return receiptId;
    }
}
