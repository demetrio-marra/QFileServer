namespace QFileServer.Mvc.ViewModels
{
    public class FileViewModel
    {
        public long Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
        public string HRSize { get; set; } = string.Empty;
    }
}
