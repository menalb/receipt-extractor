namespace ReceiptCommands.Handlers;

internal class Mappings
{
    public static TResult Map<TInput, TResult>(TInput input, TResult ifEmpty, System.Func<TInput, TResult> map)
     => input is null ? ifEmpty : map(input);
}