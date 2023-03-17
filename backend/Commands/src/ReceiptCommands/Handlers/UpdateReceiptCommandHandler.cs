using System.Threading.Tasks;
using ReceiptCommand.Model;

namespace ReceiptCommands.Handlers;

public record UpdateReceiptCommand(string ReceiptId, ReceiptDetails ReceiptDetails);

public class UpdateReceiptCommandHandler : BaseReceiptCommandHandler, ICommandHandler<UpdateReceiptCommand, ReceiptId>
{
    private readonly ISaveReceiptsGateway _receiptGateway;

    public UpdateReceiptCommandHandler(ISaveReceiptsGateway receiptGateway)
    {
        _receiptGateway = receiptGateway;
    }

    public async Task<ReceiptId> Handle(string userId, UpdateReceiptCommand command)
    {
        var updateReceipt = MapCommand(userId, command.ReceiptId, command.ReceiptDetails);

        await _receiptGateway.SaveAsync(updateReceipt);

        return new ReceiptId(command.ReceiptId);
    }
}
