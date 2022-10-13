using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QFileServer.Mvc.DTOs;
using QFileServer.Mvc.Helpers;
using QFileServer.Mvc.Services;
using QFileServer.Mvc.ViewModels;

namespace QFileServer.Mvc.Controllers
{
    public class BrowserController : Controller
    {
        private readonly ILogger<BrowserController> _logger;
        readonly IQFileServerApiService apiService;
        readonly IMapper mapper;

        public BrowserController(ILogger<BrowserController> logger,
            IQFileServerApiService apiService, 
            IMapper mapper)
        {
            _logger = logger;
            this.apiService = apiService;   
            this.mapper = mapper;
        }

        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await RefreshViewModel());
        }

        [HttpPost("Index")]
        public async Task<IActionResult> Index(BrowserViewModel vm)
        {
            var previous = GetOrInitGridPreferences();
            // reset to first page when
            // 1. page size changes
            // 2. search criteria changes
            if (previous.PageSize != vm.PageSize
                || string.Compare(previous.FilterSearchText ?? "", vm.FilterSearchText ?? "") != 0)
                vm.PageNumber = 1; 

            SaveGridPreferences(vm);
            return View("Index", await RefreshViewModel());
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(long id)
        {
            await apiService.DeleteFile(id);            
            return View("Index", await RefreshViewModel());
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload([FromForm]UploadFileViewModel uvm)
        {
            if (ModelState.IsValid)
                await apiService.UploadFile(uvm.FormFile);

            return View("Index", await RefreshViewModel());
        }

        [HttpGet("Download/{id:int}")]
        public async Task<IActionResult> Download(long id)
        {
            var downloadFileModel = await apiService.DownloadFile(id);
            return File(downloadFileModel.FileStream, downloadFileModel.ContentType, 
                downloadFileModel.FileName);
        }

        async Task<BrowserViewModel> RefreshViewModel()
        {
            var gridPreferences = GetOrInitGridPreferences();
            var oDataFilter = ODataFilter(gridPreferences);
            var oDataQueryResult = await apiService.ODataGetFiles(oDataFilter);
            return CreateViewModel(gridPreferences, oDataQueryResult);
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

        static string ODataFilter(IBrowserGridPreferences gridPreferences)
            => QFileServerHelper.ODataFilter(gridPreferences.PageSize, gridPreferences.PageNumber,
                gridPreferences.OrderByColumn, gridPreferences.OrderByAsc, gridPreferences.FilterColumn,
                gridPreferences.FilterSearchText);

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
