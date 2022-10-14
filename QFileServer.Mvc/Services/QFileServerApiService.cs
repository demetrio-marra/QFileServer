using QFileServer.Definitions.DTOs;
using QFileServer.Mvc.Models;
using System.Text.Json;

namespace QFileServer.Mvc.Services
{
    public class QFileServerApiService
    {
        static readonly string ODATA_PATH = "/odata/v1/QFileServerOData";
        static readonly string API_PATH = "/api/v1/QFileServer";
        static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        readonly HttpClient httpClient;

        public QFileServerApiService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<ODataQFileServerDTO?> ODataGetFiles(string oDataQueryString)
        {
            var uriString = ODATA_PATH + "?" + oDataQueryString;
            var response = await httpClient.GetAsync(new Uri(uriString, UriKind.Relative));
            response.EnsureSuccessStatusCode();

            var ret = await JsonSerializer.DeserializeAsync<ODataQFileServerDTO>(await response.Content.ReadAsStreamAsync());

            return ret;
        }

        public async Task<QFileServerDTO?> UploadFile(IFormFile formFile)
        {
            var mpContent = new MultipartFormDataContent();
            using (var rs = formFile.OpenReadStream())
            {
                mpContent.Add(new StreamContent(rs), "File", Path.GetFileName(formFile.FileName));
                var response = await httpClient.PostAsync(new Uri(API_PATH, UriKind.Relative), mpContent);
                response.EnsureSuccessStatusCode();
                var ret = await JsonSerializer.DeserializeAsync<QFileServerDTO>(await response.Content.ReadAsStreamAsync(),
                    jsonSerializerOptions);
                return ret;
            }
        }
       public async Task DeleteFile(long id)
        {
            var uriString = API_PATH + "/" + id.ToString();
            var response = await httpClient.DeleteAsync(new Uri(uriString, UriKind.Relative));
            response.EnsureSuccessStatusCode();
        }

        public async Task<FileDownloadModel> DownloadFile(long id)
        {
            var uriString = $"{API_PATH}/download/{id}";
            var response = await httpClient.GetAsync(new Uri(uriString, UriKind.Relative));
           
            response.EnsureSuccessStatusCode();
           
            var contentType = response.Content.Headers?.ContentType?.MediaType ?? "octet/stream";
            var fileName = response.Content.Headers?.ContentDisposition?.FileNameStar;

            return new FileDownloadModel
            {
                ContentType = contentType,
                FileName = fileName,
                FileStream = await response.Content.ReadAsStreamAsync()
            };
        }
    }
}
