using System;

namespace ReceiptCommands.Handlers;

public interface IReceiptIdGenerator
{
    string Generate();
}

public class ReceiptIdGenerator : IReceiptIdGenerator
{
    public string Generate() => Guid.NewGuid().ToString();
}
