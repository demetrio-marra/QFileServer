using System.ComponentModel.DataAnnotations;

namespace QFileServer.DTOs
{
    public class FileUploadDTO
    {
        [Required]
        public IFormFile? File { get; set; }
    }
}
