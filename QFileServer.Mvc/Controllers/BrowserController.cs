using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QFileServer.Definitions.DTOs;
using QFileServer.Mvc.DTOs;
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

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var gridPreferences = GetOrInitGridPreferences();
            var oDataFilter = ODataFilter(gridPreferences);
            var oDataQueryResult = await ODataGetFilesApi(oDataFilter);
            var vm = CreateViewModel(gridPreferences, oDataQueryResult);
            return View(vm);
        }

        [HttpPost("Index")]
        public async Task<IActionResult> Index(BrowserViewModel vm)
        {
            var previous = GetOrInitGridPreferences();
            if (previous.PageSize != vm.PageSize
                || string.Compare(previous.FilterSearchText ?? "", vm.FilterSearchText ?? "") != 0)
                vm.PageNumber = 1; // reset to first page

            SaveGridPreferences(vm);

            var gridPreferences = GetOrInitGridPreferences();
            var oDataFilter = ODataFilter(gridPreferences);
            var oDataQueryResult = await ODataGetFilesApi(oDataFilter);
            var nvm = CreateViewModel(gridPreferences, oDataQueryResult);
            return View("Index", nvm);
        }

        [HttpGet("delete/{id:int}")]
        public async Task<IActionResult> Delete(long id)
        {
            await DeleteServerFileApi(id);

            var gridPreferences = GetOrInitGridPreferences();
            var oDataFilter = ODataFilter(gridPreferences);
            var oDataQueryResult = await ODataGetFilesApi(oDataFilter);
            var nvm = CreateViewModel(gridPreferences, oDataQueryResult);
            return View("Index", nvm);
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload([FromForm]UploadFileViewModel uvm)
        {
            if (!ModelState.IsValid)
                return View("Index", new BrowserViewModel());

            QFileServerDTO? uploadedFile = await UploadServerFileApi(uvm.FormFile);

            // TODO popup with added file name & id

            var gridPreferences = GetOrInitGridPreferences();
            var oDataFilter = ODataFilter(gridPreferences);
            var oDataQueryResult = await ODataGetFilesApi(oDataFilter);
            var nvm = CreateViewModel(gridPreferences, oDataQueryResult);
            return View("Index", nvm);
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

        async Task<BrowserViewModel> RefreshGrid()
        {
            var gridPreferences = GetOrInitGridPreferences();
            var oDataFilter = ODataFilter(gridPreferences);

            var oDataQueryResult = await ODataGetFilesApi(oDataFilter);
            var vm = CreateViewModel(gridPreferences, oDataQueryResult);
            return vm;
        }

        BrowserViewModel CreateViewModel(IBrowserGridPreferences gridPreferences,
            ODataQFileServerModelDTO oDataQueryResult)
        {
            var ret = new BrowserViewModel
            {
                Files = mapper.Map<IEnumerable<FileViewModel>>(oDataQueryResult.Items),
                TotalFilesCount = oDataQueryResult.Count,
                FilterColumn = gridPreferences.FilterColumn,
                FilterSearchText = gridPreferences.FilterSearchText,
                OrderByAsc = gridPreferences.OrderByAsc,
                OrderByColumn = gridPreferences.OrderByColumn,
                PageNumber = gridPreferences.PageNumber,
                PageSize = gridPreferences.PageSize
            };

            return ret;
        }

        string ODataFilter(IBrowserGridPreferences gridPreferences)
        {
            var ret = "";
            var topClause = $"$top={gridPreferences.PageSize}";
            var skipClause = $"$skip={gridPreferences.PageSize * (gridPreferences.PageNumber - 1)}";
            var orderByClause = $"$orderby={gridPreferences.OrderByColumn} {(gridPreferences.OrderByAsc ? "asc" : "desc")}";
            var countClause = "$count=true";

            ret = string.Join("&", topClause, skipClause, orderByClause, countClause);

            if (!string.IsNullOrWhiteSpace(gridPreferences.FilterSearchText))
                ret = string.Join("&", ret, 
                    $"$filter=contains({HttpUtility.UrlEncode(gridPreferences.FilterColumn)}, '{HttpUtility.UrlEncode(gridPreferences.FilterSearchText)}')");

            return ret;
        }

        async Task<ODataQFileServerModelDTO?> ODataGetFilesApi(string odatafilter)
        {
            var client = _httpClientFactory.CreateClient(Constants.ODataQFileServerHttpClientName);
            var uriString = "?" + odatafilter;
            var result = await client.GetAsync(new Uri(uriString, UriKind.Relative));
            result.EnsureSuccessStatusCode();

            var responseString = await result.Content.ReadAsStringAsync();
            var ret = JsonSerializer.Deserialize<ODataQFileServerModelDTO>(responseString);

            return ret;
        }

        async Task DeleteServerFileApi(long id)
        {
            var client = _httpClientFactory.CreateClient(Constants.QFileServerHttpClientName);
            var uriString = id.ToString();
            var result = await client.DeleteAsync(new Uri(uriString, UriKind.Relative));
            result.EnsureSuccessStatusCode();
        }

        async Task<QFileServerDTO?> UploadServerFileApi(IFormFile ff)
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

        IBrowserGridPreferences? GetOrInitGridPreferences()
        {
            if (HttpContext.Session.GetString("GRID_PageNumber") == null)
                SaveGridPreferences(new BrowserViewModel()); // init default settings
            return GetGridPreferences();
        }

        void SaveGridPreferences(IBrowserGridPreferences vm)
        {
            HttpContext.Session.SetInt32("GRID_PageNumber", vm.PageNumber);
            HttpContext.Session.SetInt32("GRID_PageSize", vm.PageSize);
            HttpContext.Session.SetString("GRID_OrderByColumn", vm.OrderByColumn);
            HttpContext.Session.SetString("GRID_OrderByAsc", vm.OrderByAsc.ToString());
            HttpContext.Session.SetString("GRID_FilterColumn", vm.FilterColumn ?? "");
            HttpContext.Session.SetString("GRID_FilterSearchText", vm.FilterSearchText ?? "");
        }

        IBrowserGridPreferences? GetGridPreferences()
        {
            if (HttpContext.Session.GetString("GRID_PageNumber") == null)
                return null;

            var ret = new BrowserViewModel
            {
                PageNumber = HttpContext.Session.GetInt32("GRID_PageNumber")!.Value,
                PageSize = HttpContext.Session.GetInt32("GRID_PageSize")!.Value,
                OrderByColumn = HttpContext.Session.GetString("GRID_OrderByColumn")!,
                OrderByAsc = Convert.ToBoolean(HttpContext.Session.GetString("GRID_OrderByAsc")!),
                FilterColumn = HttpContext.Session.GetString("GRID_FilterColumn"),
                FilterSearchText = HttpContext.Session.GetString("GRID_FilterSearchText")
            };

            return ret;
        }
    }
}
