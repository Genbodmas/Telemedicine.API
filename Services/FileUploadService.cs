using System.Net.Http.Headers;
using System.Text.Json;

namespace Telemedicine.API.Services
{
    public class FileUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _uploadUrl;

        public FileUploadService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _uploadUrl = configuration["FileUploadSettings:Url"] ?? "http://207.180.246.69:3050/api/FileUpload/FileUpload";
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            using (var content = new MultipartFormDataContent())
            {
                using (var stream = file.OpenReadStream())
                {
                    var fileContent = new StreamContent(stream);
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                    content.Add(fileContent, "file", file.FileName);
                    
                    return await SendRequestAsync(content);
                }
            }
        }

        public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                throw new ArgumentException("File is empty");

            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
                content.Add(fileContent, "file", fileName);

                return await SendRequestAsync(content);
            }
        }

        private async Task<string> SendRequestAsync(MultipartFormDataContent content)
        {
            var response = await _httpClient.PostAsync(_uploadUrl, content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            
            if (doc.RootElement.TryGetProperty("url", out var urlElement))
            {
                return urlElement.GetString() ?? string.Empty;
            }
            
            throw new Exception("Invalid response from upload service");
        }
    }
}
