using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using QFileServer.Definitions.DTOs;

namespace QFileServer.Controllers
{
    /// <summary>
    /// Controller odata (get)
    /// </summary>
    public class QFileServerODataController : ODataController
    {
        readonly QFileServerService service;
        readonly IMapper mapper;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="service"></param>
        /// <param name="mapper"></param>
        public QFileServerODataController(QFileServerService service,
            IMapper mapper)
        {
            this.service = service;
            this.mapper = mapper;
        }

        /// <summary>
        /// OData get endpoint (max 1000 record)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Top | AllowedQueryOptions.Count
            | AllowedQueryOptions.Filter | AllowedQueryOptions.OrderBy | AllowedQueryOptions.Skip
            | AllowedQueryOptions.Top)]
        public ActionResult<IQueryable<QFileServerDTO>> Get()
            => Ok(mapper.ProjectTo<QFileServerDTO>(service.GetAllFilesOData()));
    }
}
