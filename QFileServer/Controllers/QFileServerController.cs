using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using QFileServer.DTOs;
using QFileServer.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace QFileServer.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class QFileServerController : ControllerBase
    {
        readonly QFileServerService service;
        readonly FileExtensionContentTypeProvider contentTypeProvider;
        readonly IMapper mapper;

        public QFileServerController(QFileServerService service,
            FileExtensionContentTypeProvider contentTypeProvider,
            IMapper mapper)
        {
            this.service = service;
            this.contentTypeProvider = contentTypeProvider; 
            this.mapper = mapper;
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IEnumerable<FileDTO>), description: "File model in repository")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, description: "No files in repository")]
        public async Task<IActionResult> GetAll()
        {
            var models = await service.GetAllFiles();
            if (models.Count() == 0)
                return NoContent();
            var dtos = mapper.Map<IEnumerable<FileDTO>>(models);
            return Ok(dtos);
        }

        [HttpGet]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(FileDTO), description: "File model in repository")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, description: "File id not found in repository")]
        public async Task<IActionResult> Get(long id)
        {
            var fileModel = await service.GetFile(id);
            if (fileModel == null)
                return NotFound();

            return Ok(mapper.Map<FileDTO>(fileModel));
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(FileDTO), description: "File successfully created in repository")]
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

            return Ok(mapper.Map<FileDTO>(createdModel));
        }

        [HttpPut]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.NoContent, description: "File successfull updated in repository")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, description: "File id not found in repository")]
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
        [SwaggerResponse((int)HttpStatusCode.NoContent, description: "File successfull deleted in repository")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, description: "File id not found in repository")]
        public async Task<IActionResult> Delete(long id)
        {
            if (!await service.DeleteFile(id))
                return NotFound();
            return NoContent();
        }

        [HttpGet]
        [Route("download/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, description: "File successfully downloaded")]
        [SwaggerResponse((int)HttpStatusCode.NotFound, description: "File id not found in repository")]
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
