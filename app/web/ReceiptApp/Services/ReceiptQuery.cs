using ReceiptApp.Model;

namespace ReceiptApp.Services
{
    public class ReceiptQuery
    {
        private const string receipts_url = "receipts";
        private readonly ReceiptApi _api;
        public ReceiptQuery(ReceiptApi api)
        {
            _api = api;
        }

        public async Task<IEnumerable<ReceiptSummary>> GetAll()
        {
            var receipts = await _api.Get<ReceiptsResult>(receipts_url);

            return receipts?.Receipts ?? new List<ReceiptSummary>();
        }

        public async Task<ReceiptDetails?> GetReceipt(string receiptId)
        {
            // var mocked = new ReceiptDetails
            // {
            //     Day = "11/21/2021",
            //     Id = "35ea0471-f2d8-467d-860e-e353dad2b522",
            //     Shop = new Shop
            //     {
            //         Name = "CIN CIN SNC",
            //         Owner = "CIN CIN SNC",
            //         Address = "VIA EMILIA 52",
            //         VAT = "02865321208",
            //         City = "SAN LAZZARO DI SAVENA"
            //     },
            //     Items = new List<ReceiptItem>
            //     {
            //         new ReceiptItem{
            //             Name="TE' / CAPPUCCINO 10%",
            //             VAT=10,
            //             Price=1.40M
            //         },
            //         new ReceiptItem{
            //             Name="PASTA 10%",
            //             VAT=10,
            //             Price=1.10M
            //         }
            //     },
            //     Total = 2.50M
            // };

            // return await Task.FromResult<ReceiptDetails>(mocked);
            return (await _api.Get<ReceiptDetailsResult>($"{receipts_url}/receipt/{receiptId}"))?.Receipt ?? null;
        }
    }
}