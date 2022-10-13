using System.ComponentModel.DataAnnotations;

namespace QFileServer.Mvc.ViewModels
{
    public class UploadFileViewModel : IUploadFileViewModel
    {
        [Required]
        public IFormFile? FormFile { get; set; }
    }
}
