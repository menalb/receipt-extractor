using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using ReceiptCommand.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReceiptCommands.Handlers
{
    public class RegisterReceiptCommand
    {
        public RegisterReceiptCommand(ReceiptDetails receiptDetails)
        {
            ReceiptDetails = receiptDetails;
        }
        public ReceiptDetails ReceiptDetails { get; }
    }

    internal class RegisterReceiptCommandHandler : ICommandHandler<RegisterReceiptCommand>
    {
        private readonly IAmazonDynamoDB _dynamoDBClient;

        public RegisterReceiptCommandHandler(IAmazonDynamoDB dynamoDBClient)
        {
            _dynamoDBClient = dynamoDBClient;
        }

        public async Task Handle(string userId, RegisterReceiptCommand command)
        {
            var receiptId = Guid.NewGuid().ToString();
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
        }
    }
}
