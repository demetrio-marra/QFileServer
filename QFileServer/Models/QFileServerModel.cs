namespace QFileServer.Models
{
    public class QFileServerModel
    {
        public long Id { get; set; }
        public string FullFilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
