using System.ComponentModel.DataAnnotations;

namespace QFileServer.Mvc.ViewModels
{
    public class UploadFileViewModel
    {
        [Required]
        public IFormFile FormFile { get; set; }
    }
}
