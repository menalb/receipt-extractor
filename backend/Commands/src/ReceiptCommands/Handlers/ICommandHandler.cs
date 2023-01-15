using System.Threading.Tasks;

namespace ReceiptCommands.Handlers;

public interface ICommandHandler<TCommand>
{
    Task Handle(string userId, TCommand command);
}
