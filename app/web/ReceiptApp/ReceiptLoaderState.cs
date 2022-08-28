public class ReceiptLoaderState
{
    public Action<string> OnLoadReceiptStarted { get; set; }
    public Action<string> OnLoadReceiptEnded { get; set; }

    public void SetLoading(string fileName)
    {
        OnLoadReceiptStarted?.Invoke(fileName);
    }

    public void SetLoaded(string receiptId)
    {
        OnLoadReceiptEnded?.Invoke(receiptId);
    }
}