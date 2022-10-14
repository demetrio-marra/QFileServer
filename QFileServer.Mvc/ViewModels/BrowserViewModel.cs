using QFileServer.Definitions.DTOs;
using System.ComponentModel.DataAnnotations;

namespace QFileServer.Mvc.ViewModels
{
    public class BrowserViewModel: IBrowserGridPreferences
    {
        public IEnumerable<FileViewModel>? Files { get; set; } = Enumerable.Empty<FileViewModel>();

        [Required]
        [Range(1, 9999999999)]
        public int PageNumber { get; set; } = 1;

        [Required]
        [Range(1, 100)]
        public int PageSize { get; set; } = 5;

        [Required]
        public string OrderByColumn { get; set; } = "Id";

        [Required]
        public bool OrderByAsc { get; set; } = true;

        public string? FilterColumn { get; set; } = "FileName";
        public string? FilterSearchText { get; set; } = null;

        public int LastPageNumber { get => Helpers.QFileServerHelper.CalcAvailablePages(TotalFilesCount, PageSize); }
        public long TotalFilesCount { get; set; } = 0;

        public IEnumerable<int> PageSizes { get => Enumerable.Range(5, 50).Where(x => x % 5 == 0); }
        public IEnumerable<string> OrderByColumns
        {
            get => new List<string>
            {
                "Id",
                "FileName",
                "Size"
            };
        }

        public bool DisplayAlert { get; set; } = false;
        public string? AlertType { get; set; }
        public string? AlertMessageText { get; set; }
    }
}
