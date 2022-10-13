namespace QFileServer.Mvc.ViewModels
{
    public interface IBrowserGridPreferences
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public string OrderByColumn { get; set; }

        public bool OrderByAsc { get; set; }

        public string? FilterColumn { get; set; }
        public string? FilterSearchText { get; set; }
    }
}
