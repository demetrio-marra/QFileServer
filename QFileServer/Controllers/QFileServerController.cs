using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
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
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(IQueryable<FileDTO>), description: "Files in repository")]
        [EnableQuery]
        public IQueryable<FileDTO> GetAll()
        {
            // TODO:
            // Make sure about max number of records returned
            // even when no top clause is provided
            var models = service.GetAllFilesOData();
            return mapper.ProjectTo<FileDTO>(models);
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
            var realFileName = Path.GetFileName(f.FileName);
            
            using (var readStream = f.OpenReadStream())
            {
                createdModel = await service.AddFile(new QFileServerModel
                {
                    FileName = realFileName
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

        public class WeatherForecast
        {
            public DateTime Date { get; set; }

            public int TemperatureC { get; set; }

            public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

            public string Summary { get; set; }
        }
    }
}
