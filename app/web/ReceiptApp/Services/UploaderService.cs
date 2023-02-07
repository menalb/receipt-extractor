using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;

namespace ReceiptApp.Services
{
    public class ImageService
    {
        private readonly TokenService _tokenService;
        private readonly HttpClient _httpClient;

        private readonly ReceiptLoaderStateService _state;

        public ImageService(TokenService tokenService, HttpClient httpClient, ReceiptLoaderStateService state)
        {
            _tokenService = tokenService;
            _httpClient = httpClient;
            _state = state;
        }

        public async Task<string> Upload(IBrowserFile file)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "upload-receipt");
            request.Headers.Authorization = await _tokenService.BuildAuthHeader();

            var maxFileSize = 512000 * 20;

            var response = (await _httpClient.SendAsync(request));
            if (response.IsSuccessStatusCode)
            {
                var url = (
                    await System.Net.Http.Json.HttpContentJsonExtensions.ReadFromJsonAsync<ApiResponse>(response.Content)
                    )?.UploadURL ?? throw new Exception("Unable to Upload the file");

                Console.WriteLine(file.Name);

                using var content = new MultipartFormDataContent();

                var fileContent = new StreamContent(file.OpenReadStream(maxFileSize));

                content.Add(content: fileContent);

                using var c = new ByteArrayContent(await fileContent.ReadAsByteArrayAsync());
                c.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                using var h = new HttpClient();

                var putResponse = await h.PutAsync(url, c);

                var newUploadResults = await putResponse.Content.ReadAsStringAsync();

                Console.WriteLine(newUploadResults);

                return newUploadResults;
            }
            throw new Exception("Unable do upload the file");
        }

        public async Task<string> GetImageURL(string receiptId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "upload-receipt/" + receiptId);
            request.Headers.Authorization = await _tokenService.BuildAuthHeader();

            var response = (await _httpClient.SendAsync(request));
            if (response.IsSuccessStatusCode)
            {
                var url = (
                    await System.Net.Http.Json.HttpContentJsonExtensions.ReadFromJsonAsync<ApiResponse>(response.Content)
                    )?.UploadURL ?? throw new Exception("Unable to Upload the file");
                return url;
            }
            return "";
        }
    }
    class ApiResponse
    {
        public string Key { get; set; } = "";
        public string UploadURL { get; set; } = "";
    }
}