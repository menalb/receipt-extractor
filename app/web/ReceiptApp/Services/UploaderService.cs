using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;

namespace ReceiptApp.Services
{
    public class UploaderService
    {
        private readonly TokenService _tokenService;
        private readonly HttpClient _httpClient;

        private readonly ReceiptLoaderState _state;

        public UploaderService(TokenService tokenService, HttpClient httpClient, ReceiptLoaderState state)
        {
            _tokenService = tokenService;
            _httpClient = httpClient;
            _state = state;
        }

        public async Task Upload(IBrowserFile file)
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

                _state.SetLoading(newUploadResults);

                Console.WriteLine(newUploadResults);
            }
        }
    }
    class ApiResponse
    {
        public string Key { get; set; } = "";
        public string UploadURL { get; set; } = "";
    }
}