using AutoMapper;
using QFileServer.Data.Entities;
using QFileServer.Definitions.DTOs;
using QFileServer.Models;

namespace QFileServer.Configuration
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<QFileServerEntity, QFileServerModel>()
                .ReverseMap();
            CreateMap<QFileServerModel, FileDTO>()
                .ReverseMap();
        }
    }
}
