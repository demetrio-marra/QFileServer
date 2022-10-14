using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using QFileServer.Definitions.DTOs;
using QFileServer.Mvc.Helpers;
using QFileServer.Mvc.Services;
using QFileServer.Mvc.ViewModels;
using System.Web;

namespace QFileServer.Mvc.Controllers
{
    public class BrowserController : Controller
    {
        private readonly ILogger<BrowserController> _logger;
        readonly QFileServerApiService apiService;
        readonly IMapper mapper;

        public BrowserController(ILogger<BrowserController> logger,
            QFileServerApiService apiService, 
            IMapper mapper)
        {
            _logger = logger;
            this.apiService = apiService;   
            this.mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var gridPreferences = GetOrInitGridPreferences();
            var newVM = CreateViewModel(gridPreferences);

            try
            {
                var oDataFilter = ODataFilter(gridPreferences);
                var oDataQueryResult = await apiService.ODataGetFiles(oDataFilter);
                newVM = CreateViewModel(gridPreferences, oDataQueryResult);
            }
            catch (Exception ex)
            {
                newVM.AlertMessageText = "File fetch failed : " + HttpUtility.HtmlEncode(ex.Message);
                newVM.AlertType = "alert-danger";
                newVM.DisplayAlert = true;
            }
            return View(newVM);
        }

        [HttpPost]
        public async Task<IActionResult> Index(BrowserViewModel vm)
        {
            var gridPreferences = GetOrInitGridPreferences();
            var newVM = CreateViewModel(gridPreferences);

            if (!ModelState.IsValid)
            {               
                newVM.AlertMessageText = "Bad parameters";
                newVM.AlertType = "alert-danger";
                newVM.DisplayAlert = true;
                return View(newVM);
            }
            
            // reset to first page when
            // 1. page size changes
            // 2. search criteria changes
            if (gridPreferences.PageSize != vm.PageSize
                || string.Compare(gridPreferences.FilterSearchText ?? "", vm.FilterSearchText ?? "") != 0)
                vm.PageNumber = 1;

            SaveGridPreferences(vm);

            gridPreferences = GetOrInitGridPreferences();
            var oDataFilter = ODataFilter(gridPreferences);

            try
            {
                var oDataQueryResult = await apiService.ODataGetFiles(oDataFilter);
                newVM = CreateViewModel(gridPreferences, oDataQueryResult);
                return View(newVM);
            } 
            catch (Exception ex)
            {
                newVM.AlertMessageText = "File fetch failed : " + HttpUtility.HtmlEncode(ex.Message);
                newVM.AlertType = "alert-danger";
                newVM.DisplayAlert = true;
                return View(newVM);
            }
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(long id)
        {
            var gridPreferences = GetOrInitGridPreferences();
            var newVM = CreateViewModel(gridPreferences);

            try
            {
                await apiService.DeleteFile(id);

                var oDataFilter = ODataFilter(gridPreferences);
                var oDataQueryResult = await apiService.ODataGetFiles(oDataFilter);
                newVM = CreateViewModel(gridPreferences, oDataQueryResult);

                newVM.AlertMessageText = $"File id {id} deleted successfully.";
                newVM.AlertType = "alert-info";
                newVM.DisplayAlert = true;
            } 
            catch (Exception ex)
            {
                newVM.AlertMessageText = $"File id {id} delete failed: {HttpUtility.HtmlEncode(ex.Message)}";
                newVM.AlertType = "alert-danger";
                newVM.DisplayAlert = true;
            }

            return View("Index", newVM);
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> Upload([FromForm]UploadFileViewModel uvm)
        {
            var gridPreferences = GetOrInitGridPreferences();
            BrowserViewModel newVM = CreateViewModel(gridPreferences);

            if (!ModelState.IsValid)
            {
                newVM.AlertMessageText = "Bad parameters";
                newVM.AlertType = "alert-danger";
                newVM.DisplayAlert = true;
                return View(newVM);
            }
            
            var fileDisplayName = HttpUtility.HtmlEncode(Path.GetFileName(uvm.FormFile.FileName));

            try
            {
                var ret = await apiService.UploadFile(uvm.FormFile);

                var oDataFilter = ODataFilter(gridPreferences);
                var oDataQueryResult = await apiService.ODataGetFiles(oDataFilter);
                newVM = CreateViewModel(gridPreferences, oDataQueryResult);

                newVM.AlertMessageText = $"File {fileDisplayName} uploaded successfully with id {ret.Id}";
                newVM.AlertType = "alert-info";
                newVM.DisplayAlert = true;
            } 
            catch (Exception ex)
            {
                newVM.AlertMessageText = $"File upload failed: {HttpUtility.HtmlEncode(ex.Message)}";
                newVM.AlertType = "alert-danger";
                newVM.DisplayAlert = true;
            }

            return View("Index", newVM);
        }

        [HttpGet("Download/{id:int}")]
        public async Task<IActionResult> Download(long id)
        {
            var downloadFileModel = await apiService.DownloadFile(id);
            return File(downloadFileModel.FileStream, downloadFileModel.ContentType, 
                downloadFileModel.FileName);
        }

        BrowserViewModel CreateViewModel(IBrowserGridPreferences gridPreferences,
            ODataQFileServerDTO? oDataQueryResult = null)
        {
            var ret = new BrowserViewModel
            {
                
                FilterColumn = gridPreferences.FilterColumn,
                FilterSearchText = gridPreferences.FilterSearchText,
                OrderByAsc = gridPreferences.OrderByAsc,
                OrderByColumn = gridPreferences.OrderByColumn,
                PageNumber = gridPreferences.PageNumber,
                PageSize = gridPreferences.PageSize
            };

            if (oDataQueryResult != null)
            {
                ODataQFileServerDTO d = oDataQueryResult;
                ret.Files = mapper.Map<IEnumerable<FileViewModel>>(d.Items);
                ret.TotalFilesCount = d.Count;
            }

            return ret;
        }

        static string ODataFilter(IBrowserGridPreferences gridPreferences)
            => QFileServerHelper.BuildODataQueryString(gridPreferences.PageSize, gridPreferences.PageNumber,
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
