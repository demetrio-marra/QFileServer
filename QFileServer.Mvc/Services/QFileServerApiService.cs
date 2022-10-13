using QFileServer.Definitions.DTOs;
using QFileServer.Mvc.DTOs;
using QFileServer.Mvc.Models;
using System.Text.Json;

namespace QFileServer.Mvc.Services
{
    public class QFileServerApiService : IQFileServerApiService
    {
        readonly IHttpClientFactory httpClientFactory;

        public QFileServerApiService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        async Task<ODataQFileServerModelDTO?> IQFileServerApiService.ODataGetFiles(string oDataQueryString)
        {
            var client = httpClientFactory.CreateClient(Constants.ODataQFileServerHttpClientName);
            var uriString = "?" + oDataQueryString;
            var result = await client.GetAsync(new Uri(uriString, UriKind.Relative));
            result.EnsureSuccessStatusCode();

            var responseString = await result.Content.ReadAsStringAsync();
            var ret = JsonSerializer.Deserialize<ODataQFileServerModelDTO>(responseString);

            return ret;
        }

        async Task<QFileServerDTO?> IQFileServerApiService.UploadFile(IFormFile formFile)
        {
            var mpContent = new MultipartFormDataContent();
            using (var rs = formFile.OpenReadStream())
            {
                mpContent.Add(new StreamContent(rs), "File", Path.GetFileName(formFile.FileName));
                var client = httpClientFactory.CreateClient(Constants.QFileServerHttpClientName);
                var response = await client.PostAsync("", mpContent);
                response.EnsureSuccessStatusCode();
                var ret = await JsonSerializer.DeserializeAsync<QFileServerDTO>(await response.Content.ReadAsStreamAsync());
                return ret;
            }
        }
        async Task IQFileServerApiService.DeleteFile(long id)
        {
            var client = httpClientFactory.CreateClient(Constants.QFileServerHttpClientName);
            var uriString = id.ToString();
            var result = await client.DeleteAsync(new Uri(uriString, UriKind.Relative));
            result.EnsureSuccessStatusCode();
        }

        async Task<FileDownloadModel> IQFileServerApiService.DownloadFile(long id)
        {
            var client = httpClientFactory.CreateClient(Constants.QFileServerHttpClientName);
            var uriString = $"download/{id}";
            var response = await client.GetAsync(new Uri(uriString, UriKind.Relative));
           
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
