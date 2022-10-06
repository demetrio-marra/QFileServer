namespace QFileServer.DTOs
{
    public class FileDTO
    {
        public long Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
    }
}
