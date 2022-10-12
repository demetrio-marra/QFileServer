using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QFileServer.Definitions.DTOs;
using QFileServer.Mvc.DTOs;
using QFileServer.Mvc.Helpers;
using QFileServer.Mvc.ViewModels;
using System.Text.Json;
using System.Web;

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

        [Route("Index")]
        public async Task<IActionResult> Index(BrowserViewModel vm)
        {
            return View(await CreateViewModel(vm));
        }

        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(long id)
        {
            await DeleteServerFile(id);
            var vm = await CreateViewModel();
            return View("Index", vm);
        }

        [Route("Upload")]
        [HttpPost]
        public async Task<IActionResult> Upload([FromForm]UploadFileViewModel uvm)
        {
            if (!ModelState.IsValid)
                return View("Index", new BrowserViewModel());

            QFileServerDTO? uploadedFile = await UploadServerFile(uvm.FormFile);

            // TODO popup with added file name & id

            var vm = await CreateViewModel();
            return View("Index", vm);
        }

        [HttpGet("download/{id:int}")]
        public async Task<IActionResult> Download(long id)
        {
            var client = _httpClientFactory.CreateClient(Constants.QFileServerHttpClientName);
            var uriString = $"download/{id}";
            var response = await client.GetAsync(new Uri(uriString, UriKind.Relative));
            response.EnsureSuccessStatusCode();
            var contentType = response.Content.Headers?.ContentType?.MediaType ?? "octet/stream";
            var fileName = response.Content.Headers?.ContentDisposition?.FileName;
            return File(await response.Content.ReadAsStreamAsync(), contentType, fileName);
        }

        async Task<BrowserViewModel> CreateViewModel(BrowserViewModel? vm = null)
        {
            vm = vm ?? new BrowserViewModel();

            var oDataFilter = ODataFilter(vm.PageSize,
                vm.PageNumber, vm.OrderByColumn!,
                vm.OrderByAsc, vm.FilterColumn,
                vm.FilterSearchText);

            var oDataQueryResult = await ODataGetFiles(oDataFilter);

            vm.Files = mapper.Map<IEnumerable<FileViewModel>>(oDataQueryResult.Items);
            var maxPage = QFileServerHelper.CalcAvailablePages(oDataQueryResult.Count, vm.PageSize);
            vm.LastPageNumber = maxPage;
            vm.AvailablePageNumbers = Enumerable.Range(1, maxPage);

            return vm;
        }

        string ODataFilter(int pageSize, int page, string orderByColumn, bool orderByAsc, 
            string? filterColumn, string? filterSearchText)
        {
            var ret = "";
            var topClause = $"$top={pageSize}";
            var skipClause = $"$skip={pageSize * (page - 1)}";
            var orderByClause = $"$orderby={orderByColumn} {(orderByAsc ? "asc" : "desc")}";
            var countClause = "$count=true";

            ret = string.Join("&", topClause, skipClause, orderByClause, countClause);

            if (!string.IsNullOrWhiteSpace(filterSearchText))
                ret = string.Join("&", ret, $"$filter=contains({HttpUtility.UrlEncode(filterColumn)}, '{HttpUtility.UrlEncode(filterSearchText)}')");

            return ret;
        }

        async Task<ODataQFileServerModelDTO?> ODataGetFiles(string odatafilter)
        {
            var client = _httpClientFactory.CreateClient(Constants.ODataQFileServerHttpClientName);
            var uriString = "?" + odatafilter;
            var result = await client.GetAsync(new Uri(uriString, UriKind.Relative));
            result.EnsureSuccessStatusCode();

            var responseString = await result.Content.ReadAsStringAsync();
            var ret = JsonSerializer.Deserialize<ODataQFileServerModelDTO>(responseString);

            return ret;
        }

        async Task DeleteServerFile(long id)
        {
            var client = _httpClientFactory.CreateClient(Constants.QFileServerHttpClientName);
            var uriString = id.ToString();
            var result = await client.DeleteAsync(new Uri(uriString, UriKind.Relative));
            result.EnsureSuccessStatusCode();
        }

        async Task<QFileServerDTO?> UploadServerFile(IFormFile ff)
        {
            var mpContent = new MultipartFormDataContent();
            using (var rs = ff.OpenReadStream())
            {
                mpContent.Add(new StreamContent(ff.OpenReadStream()), "File", ff.FileName);
                var client = _httpClientFactory.CreateClient(Constants.QFileServerHttpClientName);
                var response = await client.PostAsync("", mpContent);
                response.EnsureSuccessStatusCode();
                var ret = await JsonSerializer.DeserializeAsync<QFileServerDTO>(await response.Content.ReadAsStreamAsync());
                return ret;
            }          
        }
    }
}
