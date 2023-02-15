using System.Threading.Tasks;

namespace ReceiptCommands.Handlers;

public interface ICommandHandler<TCommand,TResponse>
{
    Task<TResponse> Handle(string userId, TCommand command);
}
