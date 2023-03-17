using ReceiptCommand.Model;
using System.Collections.Generic;
using System.Linq;

namespace ReceiptCommands.Handlers;

public class BaseReceiptCommandHandler
{
    protected static SaveReceiptDetails MapCommand(string userId, string receiptId, ReceiptDetails details)
    {
        var shop = MapShop(details.Shop);

        var items = MapItems(details.Items);

        return new SaveReceiptDetails
        {
            Id = receiptId,
            UserId = userId,
            Day = details.Day,
            JobId = details.JobId,
            Total = details.Total,
            TotalVAT = details.TotalVAT,
            Shop = shop,
            Items = items.ToList(),
            Tags = MapTags(details.Tags),
            Notes = details.Notes
        };
    }

    protected static List<string> MapTags(IEnumerable<string> tags) => Mappings.Map(tags, new List<string>(), t => t.ToList());

    protected static UpdateShop MapShop(Shop shop)
        => Mappings.Map(
            shop,
            new UpdateShop(),
            shop => new UpdateShop
            {
                Name = shop.Name,
                Owner = shop.Owner,
                Address = shop.Address,
                City = shop.City,
                Phone = shop.Phone,
                Vat = shop.Vat
            });

    protected static IEnumerable<UpdateReceiptItem> MapItems(IEnumerable<ReceiptItem> items)
        => Mappings.Map(
            items,
            new List<UpdateReceiptItem>(),
            items => items.Select(item => new UpdateReceiptItem
            {
                Name = item.Name,
                Price = item.Price,
                VAT = item.VAT
            }));
}
