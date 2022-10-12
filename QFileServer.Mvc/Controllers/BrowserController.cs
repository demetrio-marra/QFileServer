using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QFileServer.Definitions.DTOs;
using QFileServer.Mvc.ViewModels;
using System.Text.Json;

namespace QFileServer.Mvc.Controllers
{
    public class BrowserController : Controller
    {
        private readonly ILogger<BrowserController> _logger;
        readonly IHttpClientFactory _httpClientFactory;
        readonly IMapper mapper;

        public BrowserController(ILogger<BrowserController> logger, 
            IHttpClientFactory httpClientFactory, IMapper mapper)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index(BrowserViewModel vm)
        {
            var oDataFilter = ODataFilter(vm.PageSize,
                vm.PageNumber, vm.OrderByColumn!,
                vm.OrderByAsc, vm.FilterColumn,
                vm.FilterSearchText);

            var files = await FetchFiles(oDataFilter);

            vm.Files = mapper.Map<IEnumerable<FileViewModel>>(files);

            return View(vm);
        }

        [Route("download/{id}")]
        public async Task<IActionResult> Download(long id)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClientName);
            var uriString = $"qFileServer/download/{id}";
            var response = await client.GetAsync(new Uri(uriString, UriKind.Relative));
            response.EnsureSuccessStatusCode();
            var contentType = response.Content.Headers?.ContentType?.MediaType ?? "octet/stream";
            var fileName = response.Content.Headers?.ContentDisposition?.FileName;
            return File(await response.Content.ReadAsStreamAsync(), contentType, fileName);
        }

        string ODataFilter(int pageSize, int page, string orderByColumn, bool orderByAsc, 
            string? filterColumn, string? filterSearchText)
        {
            var topClause = $"$top={pageSize}";
            var skipClause = $"$skip={pageSize * (page - 1)}";
            var orderByClause = $"$orderby={orderByColumn} {(orderByAsc ? "asc" : "desc")}";
            return string.Join("&", topClause, skipClause, orderByClause);
        }

        public async Task<IEnumerable<QFileServerDTO>> FetchFiles(string odatafilter)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClientName);
            var uriString = "qFileServer?" + odatafilter;
            var result = await client.GetAsync(new Uri(uriString, UriKind.Relative));
            result.EnsureSuccessStatusCode();

            var dtos = await JsonSerializer.DeserializeAsync<IEnumerable<QFileServerDTO>>(
                await result.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            return dtos!;
        }
    }
}
