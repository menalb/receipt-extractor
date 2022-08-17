using Microsoft.JSInterop;
using OfficeOpenXml;
using ReceiptApp.Model;

namespace ReceiptApp.Services
{
    public class ReceiptExport
    {
        private readonly IJSRuntime js;

        public ReceiptExport(IJSRuntime js)
        {
            this.js = js;
        }

        public async ValueTask ToExcel(ReceiptDetails receipt)
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("My Sheet");

                sheet.Cells["A1:A9,C1"].Style.Font.Bold = true;

                FillHeader(sheet, receipt);

                sheet.Cells[2, 1].Value = "Shop";
                sheet.Cells[2, 1].Style.Font.Color.SetColor(0, 253, 126, 20);
                if (receipt.Shop is not null)
                {
                    FillShopInfo(sheet, receipt.Shop);
                }

                sheet.Cells[9, 1].Value = "Items";
                sheet.Cells[9, 1].Style.Font.Color.SetColor(0, 253, 126, 20);

                sheet.Cells["A10"].LoadFromCollection(receipt.Items);

                await SaveAs($"export-{receipt.Id}.xlsx", package.GetAsByteArray());
            }
        }

        private void FillHeader(ExcelWorksheet sheet, ReceiptDetails receipt)
        {
            sheet.Cells[1, 1].Value = "Day";
            sheet.Cells[1, 2].Value = receipt.Day;
            sheet.Cells[1, 3].Value = "Total";
            sheet.Cells[1, 4].Value = receipt.Total;
        }

        private void FillShopInfo(ExcelWorksheet sheet, Shop shop)
        {
            sheet.Cells[3, 1].Value = "Name";
            sheet.Cells[3, 2].Value = shop.Name;
            sheet.Cells[4, 1].Value = "Owner";
            sheet.Cells[4, 2].Value = shop.Owner;
            sheet.Cells[5, 1].Value = "Address";
            sheet.Cells[5, 2].Value = shop.Address;
            sheet.Cells[6, 1].Value = "City";
            sheet.Cells[6, 2].Value = shop.City;
            sheet.Cells[7, 1].Value = "Phone";
            sheet.Cells[7, 2].Value = shop.Phone;
            sheet.Cells[8, 1].Value = "VAT";
            sheet.Cells[8, 2].Value = shop.VAT;
        }

        private async ValueTask SaveAs(string filename, byte[] data)
        {
            await js.InvokeVoidAsync(
            "ReceiptApp.saveAsFile",
            filename,
            Convert.ToBase64String(data));
        }
    }
}