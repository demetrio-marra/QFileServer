using QFileServer.Definitions.DTOs;
using System.ComponentModel.DataAnnotations;

namespace QFileServer.Mvc.ViewModels
{
    public class BrowserViewModel
    {
        public IEnumerable<FileViewModel>? Files { get; set; } = Enumerable.Empty<FileViewModel>();

        [Required]
        public int PageNumber { get; set; } = 1;
        [Required]
        public int PageSize { get; set; } = 5;
        [Required]
        public string OrderByColumn { get; set; } = "Id";
        [Required]
        public bool OrderByAsc { get; set; } = true;
        public string? FilterColumn { get; set; } = "FileName";
        public string? FilterSearchText { get; set; } = null;

        public IEnumerable<int> AvailablePageNumbers { get; set; } = Enumerable.Range(1, 1);

        public int LastPageNumber { get; set; }

        public IEnumerable<int> PageSizes { get; set; } = Enumerable.Range(5, 50).Where(x => x % 5 == 0);
        public IEnumerable<string> OrderByColumns { get; set; } = new List<string>
        {
            "Id",
            "FileName",
            "Size"
        };
    }
}
