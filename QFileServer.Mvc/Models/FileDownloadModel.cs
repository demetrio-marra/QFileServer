namespace QFileServer.Mvc.Models
{
    public class FileDownloadModel
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public Stream FileStream { get; set; }
    }
}
