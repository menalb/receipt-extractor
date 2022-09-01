using Microsoft.AspNetCore.Components;

namespace ReceiptApp.Services
{
    public class ReceiptLoaderStateService
    {
        public event Action<ComponentBase, string>? OnLoadingReceiptChanged;
        public event Action<ComponentBase, string>? OnLoadedReceiptChanged;

        public bool IsLoadingReceipt { get; private set; }
        public string FileName { get; private set; } = "";

        public void NotifyLoadingReceiptChanged(ComponentBase sender, string fileName)
        {
            FileName = fileName;

            OnLoadingReceiptChanged?.Invoke(sender, FileName);
        }

        public void NotifyLoadedReceiptChanged(ComponentBase sender, string receiptId)
        {
            FileName = "";
            IsLoadingReceipt = false;
            OnLoadedReceiptChanged?.Invoke(sender, receiptId);
        }

        //TODO: do it for multiple files
        //TODO: change receiptId fron string to message
    }
}