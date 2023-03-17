using ReceiptCommand.Model;
using System.Threading.Tasks;

namespace ReceiptCommands.Handlers;
public record RegisterReceiptCommand(ReceiptDetails ReceiptDetails);

public class RegisterReceiptCommandHandler : BaseReceiptCommandHandler, ICommandHandler<RegisterReceiptCommand, ReceiptId>
{
    private readonly ISaveReceiptsGateway _receiptGateway;
    private readonly IReceiptIdGenerator _receiptIdGenerator;

    public RegisterReceiptCommandHandler(ISaveReceiptsGateway receiptGateway, IReceiptIdGenerator receiptIdGenerator)
    {
        _receiptGateway = receiptGateway;
        _receiptIdGenerator = receiptIdGenerator;
    }

    public async Task<ReceiptId> Handle(string userId, RegisterReceiptCommand command)
    {
        var receiptId = _receiptIdGenerator.Generate();

        var updateReceipt = MapCommand(userId, receiptId, command.ReceiptDetails);

        await _receiptGateway.SaveAsync(updateReceipt);

        return new ReceiptId(receiptId);
    }
}
