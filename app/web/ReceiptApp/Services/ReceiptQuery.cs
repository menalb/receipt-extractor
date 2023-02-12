using ReceiptApp.Model;

namespace ReceiptApp.Services
{
    public class ReceiptQuery
    {
        private const string receipts_url = "receipts";
        private readonly ReceiptApi _api;

        private readonly bool _useMockedReceipts = false;
        public ReceiptQuery(ReceiptApi api)
        {
            _api = api;
            _useMockedReceipts = true;
        }

        public async Task<IEnumerable<ReceiptSummary>> GetAll(DateTime? from = null)
        {
            var dayFrom = from ?? DateTime.Now
                .AddDays(-30);
            var fromParam = dayFrom.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
            var toParam = dayFrom.AddMonths(1).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

            if (_useMockedReceipts)
            {
                var receipts = System.Text.Json.JsonSerializer.Deserialize<ReceiptsResult>(mockedList);
                await Task.Delay(1000);
                return await Task.FromResult(receipts?.Receipts ?? new List<ReceiptSummary>());
            }
            else
            {
                var receipts = await _api.Get<ReceiptsResult>(receipts_url + $"?from={fromParam}&to={toParam}");
                return receipts?.Receipts ?? new List<ReceiptSummary>();
            }
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

        private string mockedList = @"{
	""Receipts"": [
		 {
			""Id"": ""00107153-818b-4ead-b95f-2f28df640be8"",
			""job_Id"": """",
			""Day"": ""2022-12-31"",
			""Shop"": ""VEGETA CAFE BISTROT"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""59753455-bb3a-46a1-9611-3aead31ad317"",
			""job_Id"": """",
			""Day"": ""2023-01-07"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc. Coop."",
			""Total"": 14.8
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""034b8e7a-a1de-4745-91ee-77abea3dcae6"",
			""job_Id"": """",
			""Day"": ""2023-01-25"",
			""Shop"": ""VEGETA CAFE BISTROT"",
			""Total"": 2.7		,
			""Tags"": [
		 ""pharmacy"" 			]},		 {
			""Id"": ""0a7eb661-29ec-43e9-973d-5f10840634fe"",
			""job_Id"": """",
			""Day"": ""2022-12-30"",
			""Shop"": ""IL PANIFICIO srls"",
			""Total"": 1.2
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""0ef6401c-35c5-4e29-afea-b48f7ce1eb57"",
			""job_Id"": """",
			""Day"": ""2022-12-29"",
			""Shop"": ""LE PLAISIR"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""12a382fb-7dc7-4b80-bda8-c2f1a5505652"",
			""job_Id"": """",
			""Day"": ""2023-01-16"",
			""Shop"": ""VEGETA CAFE BISTROT"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""135e92ef-a656-4070-bb17-f22a28e6f821"",
			""job_Id"": """",
			""Day"": ""2023-01-07"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc. Coop."",
			""Total"": 31.47
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""1d48c059-48db-4560-b03b-e4a497c47a23"",
			""job_Id"": """",
			""Day"": ""2023-01-13"",
			""Shop"": ""SUPERMERCATO COVAD"",
			""Total"": 16.94
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""1fa098cf-fb30-410e-be67-bea138e9a4ff"",
			""job_Id"": """",
			""Day"": ""2023-01-04"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc. Coop."",
			""Total"": 32.18
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""228f71d2-29cc-4f9d-8ed0-622c09a5c7b1"",
			""job_Id"": """",
			""Day"": ""2023-01-04"",
			""Shop"": ""VEGETA CAFE BISTROT"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""2356cad1-a639-4393-8c48-2c897e1bab56"",
			""job_Id"": """",
			""Day"": ""2023-01-03"",
			""Shop"": ""FARMACIA JUSSI"",
			""Total"": 11.4
		,
			""Tags"": [
		 ""pharmacy"" 			]},		 {
			""Id"": ""25a9f7be-a70e-4f47-b037-7479f044b786"",
			""job_Id"": """",
			""Day"": ""2023-01-22"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc Coop."",
			""Total"": 15.66
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""26781631-7507-46ab-afbe-ca09a3e303d4"",
			""job_Id"": """",
			""Day"": ""2023-01-11"",
			""Shop"": ""BAR BRISTOL"",
			""Total"": 1.5
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""2a49626b-ccf6-4a1e-bcb7-184ab8e6a7a0"",
			""job_Id"": """",
			""Day"": ""2022-12-31"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc. Coop."",
			""Total"": 47.07
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""3302799e-0def-40d1-9400-e1f980c21bd2"",
			""job_Id"": """",
			""Day"": ""2022-12-28"",
			""Shop"": ""CIN CIN SNC"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""343f6ea5-4ead-432f-8b8e-682aaa695496"",
			""job_Id"": """",
			""Day"": ""2022-12-27"",
			""Shop"": ""FRANTOIO EVO DEL BORGO"",
			""Total"": 71
		,
			""Tags"": [
			]},		 {
			""Id"": ""3956c4e4-95ec-42ea-9e35-704ec3de6c1e"",
			""job_Id"": """",
			""Day"": ""2023-01-24"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc. Coop."",
			""Total"": 39.68
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""40c6607b-625b-4029-acac-ae0bb583f795"",
			""job_Id"": """",
			""Day"": ""2023-01-03"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc. Coop."",
			""Total"": 9.23
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""4e472c54-fe24-40f4-9d73-3a12b7ccd665"",
			""job_Id"": """",
			""Day"": ""2023-01-11"",
			""Shop"": ""CIN CIN SNC"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""535f8fe7-4a65-43d7-812d-3a76894f4318"",
			""job_Id"": """",
			""Day"": ""2023-01-23"",
			""Shop"": ""CAFE' MARTEL"",
			""Total"": 8.3
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""53607fd2-6f63-40ab-ba3c-a8cb29c38b3e"",
			""job_Id"": """",
			""Day"": ""2022-12-30"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc. Coop."",
			""Total"": 18.71
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""548b465b-3adb-48f3-9c2e-4d5bb1c33680"",
			""job_Id"": """",
			""Day"": ""2023-01-21"",
			""Shop"": ""VEGETA CAFE BISTROT"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""67907de4-fec7-40e2-8db2-9859a8d5ae02"",
			""job_Id"": """",
			""Day"": ""2023-01-23"",
			""Shop"": ""PASTICCERIA CAFFETTERIA CAMESCO"",
			""Total"": 3.5
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""7157c865-ebc3-4572-8873-7b84cf227b34"",
			""job_Id"": """",
			""Day"": ""2023-01-09"",
			""Shop"": ""VEGETA CAFE BISTROT"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""7191edce-2e4c-4604-a6c3-d7ce1176532d"",
			""job_Id"": ""fbd1de5896529a4a27ddcbfb41df4ca09adbc93e1cf207c633fb5191819c9328"",
			""Day"": ""2023-01-23"",
			""Shop"": ""KULMBACHER BIER HAUS"",
			""Total"": 10.4
		,
			""Tags"": [
			]},		 {
			""Id"": ""74f7d5da-61fe-45dd-96e8-a2da67613331"",
			""job_Id"": """",
			""Day"": ""2023-01-19"",
			""Shop"": ""CIN CIN SNC"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""7ceb2c6a-b230-4793-bbed-81e435a96253"",
			""job_Id"": """",
			""Day"": ""2023-01-14"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc. Coop."",
			""Total"": 34.97
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""9ed4ffb4-31dc-4ad2-bc48-0058736c4584"",
			""job_Id"": """",
			""Day"": ""2022-12-29"",
			""Shop"": ""BAR BRISTOL"",
			""Total"": 1.5
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""a03d23c7-2e78-4d10-a875-22d3089de012"",
			""job_Id"": """",
			""Day"": ""2022-12-28"",
			""Shop"": ""COOP ALLEANZA 3.0 Soc. Coop."",
			""Total"": 17.64
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""a0ae0745-e622-440a-80a2-c9eb5197df9e"",
			""job_Id"": """",
			""Day"": ""2023-01-13"",
			""Shop"": ""BAR BRISTOL"",
			""Total"": 1.2
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""a233ac88-3b84-4c09-83de-2f864c182997"",
			""job_Id"": """",
			""Day"": ""2023-01-03"",
			""Shop"": ""BAR BRISTOL"",
			""Total"": 2
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""a3c308b1-d2e9-47a9-a2ff-4ba2645b1ccd"",
			""job_Id"": """",
			""Day"": ""2022-12-29"",
			""Shop"": ""PASTICCERIA REPUBBLICA"",
			""Total"": 13.5
		,
			""Tags"": [
			]},		 {
			""Id"": ""a71cbe72-1373-4718-8132-f25609ff4bf8"",
			""job_Id"": """",
			""Day"": ""2023-01-05"",
			""Shop"": ""CIN CIN SNC"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""b345111c-2cf7-467b-805f-8077239ea049"",
			""job_Id"": """",
			""Day"": ""2023-01-18"",
			""Shop"": ""LE PLAISIR"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""bea3ccd4-20bf-445c-b24b-9c2eeb00bb72"",
			""job_Id"": """",
			""Day"": ""2023-01-02"",
			""Shop"": ""SUPERMERCATO CONAD"",
			""Total"": 8.11
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""c7a2c7e5-f1b4-41f0-b810-e933806daddb"",
			""job_Id"": """",
			""Day"": ""2023-01-10"",
			""Shop"": ""MALI' CAFE'"",
			""Total"": 2.9
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""cf6dcc92-f21b-455a-8650-339df1f690af"",
			""job_Id"": """",
			""Day"": ""2023-01-18"",
			""Shop"": ""BAR BRISTOL"",
			""Total"": 1.5
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""d3bd6eeb-a4f2-481f-b35e-7ae3db93f419"",
			""job_Id"": """",
			""Day"": ""2023-01-24"",
			""Shop"": ""CIN CIN SNC"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""d5f87c16-4f99-4713-ae99-81d2ba91fd5e"",
			""job_Id"": """",
			""Day"": ""2023-01-10"",
			""Shop"": ""SUPERMERCATO CONAD"",
			""Total"": 7.19
		,
			""Tags"": [
		 ""Shop"" 			]},		 {
			""Id"": ""dc441550-46df-47b7-84f9-eb5db58ca460"",
			""job_Id"": """",
			""Day"": ""2022-12-27"",
			""Shop"": ""BARBAGLIO"",
			""Total"": 2.7
		,
			""Tags"": [
		 ""bar"" 			]},		 {
			""Id"": ""eae24df1-c54a-4510-abc8-e3b024a586bd"",
			""job_Id"": """",
			""Day"": ""2023-01-23"",
			""Shop"": ""Pizzeria La Capannina"",
			""Total"": 11.5
		,
			""Tags"": [
			]},		 {
			""Id"": ""f5a94062-5497-4b07-ae29-ce7a0cb3b55e"",
			""job_Id"": """",
			""Day"": ""2023-01-19"",
			""Shop"": ""LA SOSTA SRL"",
			""Total"": 2.8
		,
			""Tags"": [
		 ""bar"" 			]}			]
}
";
    }
}