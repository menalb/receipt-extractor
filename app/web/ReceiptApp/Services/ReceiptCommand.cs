using ReceiptApp.Model;

namespace ReceiptApp.Services
{
    public class ReceiptCommand
    {
        private const string receipts_url = "receipts";
        private readonly ReceiptApi _api;
        public ReceiptCommand(ReceiptApi api)
        {
            _api = api;
        }

        public async Task Save(string receiptId, ReceiptDetails receipt)
        {
            await _api.Put<ReceiptDetails>($"{receipts_url}/receipt/{receiptId}", receipt);
        }

        public async Task Delete(string receiptId)
        {
            await _api.Delete($"{receipts_url}/receipt/{receiptId}");
        }
    }
}