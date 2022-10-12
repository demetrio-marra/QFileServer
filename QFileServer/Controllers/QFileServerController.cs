using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using QFileServer.Definitions.DTOs;
using QFileServer.DTOs;
using QFileServer.Models;

namespace QFileServer.Controllers
{
    /// <summary>
    /// Controller principale
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class QFileServerController : ControllerBase
    {
        readonly QFileServerService service;
        readonly FileExtensionContentTypeProvider contentTypeProvider;
        readonly IMapper mapper;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="contentTypeProvider"></param>
        /// <param name="mapper"></param>
        public QFileServerController(QFileServerService service,
            FileExtensionContentTypeProvider contentTypeProvider,
            IMapper mapper)
        {
            this.service = service;
            this.contentTypeProvider = contentTypeProvider;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(typeof(QFileServerDTO), statusCode: StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(long id)
        {
            var fileModel = await service.GetFile(id);
            if (fileModel == null)
                return NotFound();

            return Ok(mapper.Map<QFileServerDTO>(fileModel));
        }

        [HttpPost]
        [ProducesResponseType(typeof(QFileServerDTO), statusCode: StatusCodes.Status200OK, "application/json")]
        public async Task<IActionResult> Add([FromForm] FileUploadDTO dto)
        {
            IFormFile f = dto.File!;
            QFileServerModel createdModel;
            using (var readStream = f.OpenReadStream())
            {
                createdModel = await service.AddFile(new QFileServerModel
                {
                    FileName = f.FileName
                }, readStream);
            }

            return Ok(mapper.Map<QFileServerDTO>(createdModel));
        }

        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(long id, [FromForm] FileUploadDTO dto)
        {
            IFormFile f = dto.File!;
            using (var readStream = f.OpenReadStream())
            {
                if (!await service.ReplaceFile(id, new QFileServerModel
                {
                    FileName = f.FileName
                }, readStream))
                    return NotFound();
            }

            return NoContent();
        }

        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            if (!await service.DeleteFile(id))
                return NotFound();
            return NoContent();
        }

        [HttpGet]
        [Route("download/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(long id)
        {
            var fileModel = await service.GetFile(id);
            if (fileModel == null)
                return NotFound();

            if (!contentTypeProvider.TryGetContentType(fileModel.FullFilePath, out string? contentType))
                contentType = "octet/stream";

            var downloadStream = new FileStream(fileModel.FullFilePath, FileMode.Open, FileAccess.Read);
            return File(downloadStream, contentType!, fileModel.FileName);
        }
    }
}
