using QFileServer.Definitions.DTOs;
using QFileServer.Mvc.DTOs;
using QFileServer.Mvc.Models;

namespace QFileServer.Mvc.Services
{
    public interface IQFileServerApiService
    {
        Task<ODataQFileServerModelDTO?> ODataGetFiles(string oDataQuery);
        Task<QFileServerDTO?> UploadFile(IFormFile formFile);
        Task DeleteFile(long id);
        Task<FileDownloadModel> DownloadFile(long id);
    }
}
